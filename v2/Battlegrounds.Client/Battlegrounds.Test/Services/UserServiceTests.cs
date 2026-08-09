using System.Buffers.Text;
using System.IO;
using System.Text;
using System.Text.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.Services.Infrastructure;

using NSubstitute;

namespace Battlegrounds.Test.Services;

/// <summary>
/// Covers the token lifecycle after sign-in.
/// </summary>
/// <remarks>The API rotates refresh tokens and answers a replayed one by revoking every session the account has, so
/// most of what is asserted here is about a token being spent exactly once and reaching disk before it is used.</remarks>
[TestOf(typeof(UserService))]
public class UserServiceTests {

    private const string RefreshEndpointErrorBreach = "RefreshToken.Breach";

    private IBattlegroundsWebAPI _webAPI;
    private IBrowserService _browserService;
    private TestLogger<UserService> _logger;
    private string _tokenStorePath;
    private List<UserService> _services;

    [SetUp]
    public void SetUp() {
        _webAPI = Substitute.For<IBattlegroundsWebAPI>();
        _browserService = Substitute.For<IBrowserService>();
        _logger = new TestLogger<UserService>();
        _tokenStorePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"user-service-{Guid.NewGuid():N}.dat");
        _services = [];
    }

    [TearDown]
    public void TearDown() {
        foreach (UserService service in _services) {
            service.Dispose(); // Stops the background refresh timer before the logger it writes to goes away
        }
        _logger.Dispose();
        if (File.Exists(_tokenStorePath)) {
            File.Delete(_tokenStorePath);
        }
    }

    private UserService CreateService() {
        UserService service = new(_logger, _webAPI, _browserService, _tokenStorePath);
        _services.Add(service);
        return service;
    }

    /// <summary>
    /// Builds an unsigned JWT carrying only an <c>exp</c> claim. <c>UserService</c> reads the payload and never
    /// verifies the signature, so nothing more is needed.
    /// </summary>
    private static string Jwt(TimeSpan expiresIn) {
        string payload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { exp = DateTimeOffset.UtcNow.Add(expiresIn).ToUnixTimeSeconds() })));
        return $"header.{payload}.signature";
    }

    private static LoginResponse LoginResponseWith(string accessToken, string refreshToken) =>
        new(accessToken, refreshToken, 3600, DateTime.UtcNow.AddHours(1), new ApiUser("bg-1", "ragnar", "Ragnar"));

    private static RefreshResult Rotated(string accessToken, string refreshToken) =>
        new(RefreshOutcome.Success, new RefreshResponse(accessToken, refreshToken, new ApiUser("bg-1", "ragnar", "Ragnar")));

    /// <summary>
    /// Signs a user in with an access token that is already inside the refresh margin, so the next token request
    /// triggers a refresh.
    /// </summary>
    private async Task<UserService> LoggedInServiceNeedingRefreshAsync(string refreshToken = "refresh-1") {
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromSeconds(30)), refreshToken));
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");
        return service;
    }

    [Test]
    public async Task LoginAsync_StoresTheTokenPairAndMarksTheUserLoggedIn() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        UserService service = CreateService();

        // Act
        User? user = await service.LoginAsync("ragnar", "hunter2");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(user, Is.Not.Null, "The signed-in user should be returned");
            Assert.That(await service.IsUserLoggedIn, Is.True, "The logged-in gate should have completed");
            Assert.That(service.GetLocalUserRefreshToken(), Is.EqualTo("refresh-1"), "The refresh token should be held");
            Assert.That(File.Exists(_tokenStorePath), Is.True, "The token store should have been written");
        }

    }

    [Test]
    public async Task GetLocalUserTokenAsync_RefreshingTwiceInOneSession_SucceedsBothTimes() {

        // A completed TaskCompletionSource used to throw on the second store, failing every in-session refresh.

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(
            Rotated(Jwt(TimeSpan.FromSeconds(30)), "refresh-2"),
            Rotated(Jwt(TimeSpan.FromHours(1)), "refresh-3"));

        // Act
        string first = await service.GetLocalUserTokenAsync();
        string second = await service.GetLocalUserTokenAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(first, Is.Not.Empty, "The first refresh should yield a token");
            Assert.That(second, Is.Not.Empty, "The second refresh should yield a token too");
            Assert.That(service.GetLocalUserRefreshToken(), Is.EqualTo("refresh-3"), "The latest rotated token should be held");
        }
        await _webAPI.Received(2).RefreshTokenAsync(Arg.Any<RefreshRequest>());

    }

    [Test]
    public async Task GetLocalUserTokenAsync_AfterARefresh_PresentsTheRotatedTokenNotTheOriginal() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(
            Rotated(Jwt(TimeSpan.FromSeconds(30)), "refresh-2"),
            Rotated(Jwt(TimeSpan.FromHours(1)), "refresh-3"));

        // Act
        await service.GetLocalUserTokenAsync();
        await service.GetLocalUserTokenAsync();

        // Assert -- replaying refresh-1 is what the API classifies as a breach
        await _webAPI.Received(1).RefreshTokenAsync(Arg.Is<RefreshRequest>(r => r.RefreshToken == "refresh-1"));
        await _webAPI.Received(1).RefreshTokenAsync(Arg.Is<RefreshRequest>(r => r.RefreshToken == "refresh-2"));

    }

    [Test]
    public async Task GetLocalUserTokenAsync_WhenCalledConcurrently_RefreshesExactlyOnce() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(async _ => {
            await Task.Delay(20); // Widen the window a racing caller would slip through
            return Rotated(Jwt(TimeSpan.FromHours(1)), "refresh-2");
        });

        // Act
        string[] tokens = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => service.GetLocalUserTokenAsync()));

        // Assert -- a second exchange of refresh-1 would revoke every session the account has
        await _webAPI.Received(1).RefreshTokenAsync(Arg.Any<RefreshRequest>());
        Assert.That(tokens, Is.All.Not.Empty, "Every caller should receive the refreshed token");

    }

    [Test]
    public async Task GetLocalUserTokenAsync_WhenRefreshSucceeds_PersistsTheRotatedTokenBeforeReturning() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(Rotated(Jwt(TimeSpan.FromHours(1)), "refresh-2"));

        // Act
        await service.GetLocalUserTokenAsync();

        // Assert -- a token that only exists in memory is lost on a crash, leaving the spent one on disk to be replayed
        UserService reloaded = CreateService();
        Assert.That(await reloaded.AutoLoginAsync(), Is.True, "The persisted session should auto-login");
        Assert.That(reloaded.GetLocalUserRefreshToken(), Is.EqualTo("refresh-2"), "The store should hold the rotated token");

    }

    [Test]
    public async Task GetLocalUserTokenAsync_WhenTheRefreshTokenIsRejected_ClearsTheLocalSession() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>())
            .Returns(new RefreshResult(RefreshOutcome.Rejected, null, RefreshEndpointErrorBreach));

        // Act
        string token = await service.GetLocalUserTokenAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(token, Is.Empty, "No token should be handed out");
            Assert.That(service.GetLocalUserRefreshToken(), Is.Empty, "The dead refresh token should be discarded");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "The token store should be deleted");
        }

    }

    [Test]
    public async Task GetLocalUserTokenAsync_WhenRefreshFailsTransiently_KeepsTheRefreshTokenForAnotherAttempt() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(new RefreshResult(RefreshOutcome.Transient, null));

        // Act
        string token = await service.GetLocalUserTokenAsync();

        // Assert -- a flaky network says nothing about the token's validity
        using (Assert.EnterMultipleScope()) {
            Assert.That(token, Is.Empty, "No token should be handed out while the refresh is failing");
            Assert.That(service.GetLocalUserRefreshToken(), Is.EqualTo("refresh-1"), "The refresh token should be retained");
            Assert.That(File.Exists(_tokenStorePath), Is.True, "The token store should survive a transient failure");
        }

    }

    [Test]
    public async Task LogOutAsync_RevokesTheSessionServerSideAndClearsEverythingLocal() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        _webAPI.LogoutAsync().Returns(true);
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");

        // Act
        bool revoked = await service.LogOutAsync();

        // Assert
        await _webAPI.Received(1).LogoutAsync();
        using (Assert.EnterMultipleScope()) {
            Assert.That(revoked, Is.True, "The revocation should be reported");
            Assert.That(service.GetLocalUserRefreshToken(), Is.Empty, "The refresh token should be cleared");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "The token store should be deleted so the next launch does not auto-login");
        }
        _webAPI.Received().SetAuthenticationToken(string.Empty);

    }

    [Test]
    public async Task LogOutAsync_WithATokenNearExpiry_RefreshesBeforeRevoking() {

        // Arrange
        UserService service = await LoggedInServiceNeedingRefreshAsync();
        string rotatedAccessToken = Jwt(TimeSpan.FromHours(1));
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(Rotated(rotatedAccessToken, "refresh-2"));
        _webAPI.LogoutAsync().Returns(true);

        // Act
        await service.LogOutAsync();

        // Assert -- the server finds the refresh token to revoke by the JTI of the presented access token, so a stale
        // one revokes a session that has already rotated away and leaves the live refresh token valid
        await _webAPI.Received(1).RefreshTokenAsync(Arg.Any<RefreshRequest>());
        Received.InOrder(() => {
            _webAPI.SetAuthenticationToken(rotatedAccessToken);
            _webAPI.LogoutAsync();
        });

    }

    [Test]
    public async Task LogOutAsync_WhenTheServerCallFails_StillClearsTheLocalSession() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        _webAPI.LogoutAsync().Returns(Task.FromException<bool>(new HttpRequestException("offline")));
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");

        // Act
        bool revoked = await service.LogOutAsync();

        // Assert -- the user asked to be signed out of this machine; that must not depend on the network
        using (Assert.EnterMultipleScope()) {
            Assert.That(revoked, Is.False, "The failed revocation should be reported");
            Assert.That(service.GetLocalUserRefreshToken(), Is.Empty, "The refresh token should be cleared anyway");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "The token store should be deleted anyway");
        }

    }

    [Test]
    public async Task WaitForPendingLogOutAsync_BlocksUntilTheRevocationCompletes() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        TaskCompletionSource<bool> revocation = new();
        _webAPI.LogoutAsync().Returns(revocation.Task);
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");

        // Act -- abandon the logout task the way closing the window would
        _ = service.LogOutAsync();
        Task waiting = service.WaitForPendingLogOutAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.That(waiting.IsCompleted, Is.False, "Shutdown should still be waiting on the revocation");
        revocation.SetResult(true);
        await waiting;
        await _webAPI.Received(1).LogoutAsync();

    }

    [Test]
    public async Task WaitForPendingLogOutAsync_WhenTheRevocationHangs_GivesUpAfterTheTimeout() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        _webAPI.LogoutAsync().Returns(new TaskCompletionSource<bool>().Task); // Never completes
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");
        _ = service.LogOutAsync();

        // Act -- a hung request must not hold the app open
        Assert.That(async () => await service.WaitForPendingLogOutAsync(TimeSpan.FromMilliseconds(200)), Throws.Nothing);

    }

    [Test]
    public async Task WaitForPendingLogOutAsync_WithNoSignOutInFlight_ReturnsImmediately() {

        // Arrange
        UserService service = CreateService();

        // Act & Assert -- the common case is closing the app while still signed in
        Task waiting = service.WaitForPendingLogOutAsync(TimeSpan.FromSeconds(5));
        Assert.That(waiting.IsCompleted, Is.True, "Shutdown should not wait when nobody signed out");
        await waiting;

    }

    [Test]
    public async Task AutoLoginAsync_WithAnUnreadableTokenStore_ReturnsFalseAndDiscardsIt() {

        // Arrange -- a store written by a different Windows account cannot be decrypted
        await File.WriteAllBytesAsync(_tokenStorePath, "not an encrypted token store"u8.ToArray());
        UserService service = CreateService();

        // Act
        bool loggedIn = await service.AutoLoginAsync();

        // Assert -- startup runs this from an async void method, so an escaping exception takes the app down
        using (Assert.EnterMultipleScope()) {
            Assert.That(loggedIn, Is.False, "Auto-login should fail rather than throw");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "The unusable store should be discarded");
        }

    }

    [Test]
    public async Task AutoLoginAsync_WithAStoredTokenNearExpiry_RefreshesItUpFront() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromSeconds(30)), "refresh-1"));
        await CreateService().LoginAsync("ragnar", "hunter2");

        _webAPI.ClearReceivedCalls();
        _webAPI.RefreshTokenAsync(Arg.Any<RefreshRequest>()).Returns(Rotated(Jwt(TimeSpan.FromHours(1)), "refresh-2"));
        UserService restarted = CreateService();

        // Act
        bool loggedIn = await restarted.AutoLoginAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(loggedIn, Is.True, "The stored session should be recovered");
            Assert.That(restarted.GetLocalUserRefreshToken(), Is.EqualTo("refresh-2"), "The rotated token should be held");
        }
        await _webAPI.Received(1).RefreshTokenAsync(Arg.Is<RefreshRequest>(r => r.RefreshToken == "refresh-1"));

    }

    [Test]
    public async Task GetLocalUserTokenAsync_WithAValidToken_DoesNotRefresh() {

        // Arrange
        _webAPI.LoginAsync(Arg.Any<LoginRequest>()).Returns(LoginResponseWith(Jwt(TimeSpan.FromHours(1)), "refresh-1"));
        UserService service = CreateService();
        await service.LoginAsync("ragnar", "hunter2");

        // Act
        string token = await service.GetLocalUserTokenAsync();

        // Assert
        Assert.That(token, Is.Not.Empty, "The existing token should be handed straight back");
        await _webAPI.DidNotReceive().RefreshTokenAsync(Arg.Any<RefreshRequest>());

    }

}
