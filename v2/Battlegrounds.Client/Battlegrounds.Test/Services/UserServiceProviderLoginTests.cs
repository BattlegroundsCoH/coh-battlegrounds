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
/// Covers the browser sign-in, and specifically how it composes the loopback listener with the poll.
/// </summary>
/// <remarks>The API <i>ignores</i> a return URL it does not allow rather than refusing it: the sign-in still
/// succeeds, the browser lands on the API's own page, and the listener never fires -- with nothing anywhere saying
/// why. That is why the poll starts alongside the browser and stays authoritative, and it is what most of these
/// tests exist to pin.</remarks>
[TestOf(typeof(UserService))]
public class UserServiceProviderLoginTests {

    private IBattlegroundsWebAPI _webAPI;
    private IBrowserService _browserService;
    private FakeLoopbackAuthListenerFactory _listenerFactory;
    private IWindowActivationService _windowActivation;
    private TestLogger<UserService> _logger;
    private string _tokenStorePath;
    private List<UserService> _services;

    [SetUp]
    public void SetUp() {
        _webAPI = Substitute.For<IBattlegroundsWebAPI>();
        _browserService = Substitute.For<IBrowserService>();
        _windowActivation = Substitute.For<IWindowActivationService>();
        _listenerFactory = new FakeLoopbackAuthListenerFactory();
        _logger = new TestLogger<UserService>();
        _tokenStorePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"user-service-provider-{Guid.NewGuid():N}.dat");
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

    private UserService CreateService(ILoopbackAuthListenerFactory? listenerFactory = null) {
        UserService service = new(_logger, _webAPI, _browserService, _tokenStorePath, listenerFactory ?? _listenerFactory, _windowActivation);
        _services.Add(service);
        return service;
    }

    /// <summary>
    /// Builds an unsigned JWT carrying only an <c>exp</c> claim, which is all <c>UserService</c> reads.
    /// </summary>
    private static string Jwt(TimeSpan expiresIn) {
        string payload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { exp = DateTimeOffset.UtcNow.Add(expiresIn).ToUnixTimeSeconds() })));
        return $"header.{payload}.signature";
    }

    private void ArrangeStart(string sessionId = "session-1")
        => _webAPI.StartAuthAsync(Arg.Any<AuthProvider>(), Arg.Any<string?>())
            .Returns(new StartAuthResponse(sessionId, "https://discord.com/oauth2/authorize", "verifier-1"));

    private void ArrangePoll(AuthStatusResult result)
        => _webAPI.EndAuthAsync(Arg.Any<AuthProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(result);

    private static AuthStatusResult SignedIn() => new(AuthStatusOutcome.Success, new EndAuthResponse(
        Jwt(TimeSpan.FromHours(1)), "refresh-1", new ApiUser("bg-1", "ragnar", "Ragnar"), 3600, DateTime.UtcNow.AddHours(1)));

    [Test]
    public async Task LoginWithDiscordAsync_PassesTheListenersReturnUrlToStart() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        _listenerFactory.ReturnUrl = "http://127.0.0.1:54321/auth/callback";

        // Act
        await CreateService().LoginWithDiscordAsync();

        // Assert
        await _webAPI.Received(1).StartAuthAsync(AuthProvider.Discord, "http://127.0.0.1:54321/auth/callback");

    }

    [Test]
    public async Task LoginWithDiscordAsync_OpensTheAuthLinkInTheBrowser() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());

        // Act
        await CreateService().LoginWithDiscordAsync();

        // Assert
        _browserService.Received(1).OpenUrl("https://discord.com/oauth2/authorize");

    }

    [Test]
    public async Task LoginWithSteamAsync_OnSuccess_AppliesTheTokensAndWritesTheStore() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        UserService service = CreateService();

        // Act
        User user = await service.LoginWithSteamAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(user.UserId, Is.EqualTo("bg-1"), "The signed-in user should come from the API response");
            Assert.That(user.UserDisplayName, Is.EqualTo("Ragnar"), "The display name should come from the API response");
            Assert.That(service.GetLocalUserRefreshToken(), Is.EqualTo("refresh-1"), "The refresh token should be held");
            Assert.That(File.Exists(_tokenStorePath), Is.True, "The token store should have been written before the call returned");
        }

    }

    /// <summary>
    /// No listener is a supported state, not a failure: it is what a machine that cannot bind a loopback port gets,
    /// and what the configuration switch produces.
    /// </summary>
    [Test]
    public async Task LoginWithDiscordAsync_WhenNoListenerCanBind_StartsWithNoReturnUrlAndStillSignsIn() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        _listenerFactory.Listener = null;

        // Act
        User user = await CreateService().LoginWithDiscordAsync();

        // Assert
        Assert.That(user.UserId, Is.EqualTo("bg-1"), "The sign-in should complete on the poll alone");
        await _webAPI.Received(1).StartAuthAsync(AuthProvider.Discord, null);

    }

    /// <summary>
    /// The degradation that the whole design exists to survive. A return URL the API's allowlist rejects is ignored
    /// rather than refused, so the listener never fires -- and because a completed session's payload lives only a
    /// minute, a design that waited on the listener before polling would find nothing left to collect.
    /// </summary>
    [Test]
    public async Task LoginWithDiscordAsync_WhenTheCallbackNeverArrives_StillSignsInFromThePoll() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        _listenerFactory.Listener!.NeverCallsBack();

        // Act
        User user = await CreateService().LoginWithDiscordAsync();

        // Assert
        Assert.That(user.UserId, Is.EqualTo("bg-1"), "The poll should complete the sign-in without the listener ever firing");

    }

    /// <summary>
    /// The state is not a secret -- it travels to the identity provider and back -- and the API releases nothing
    /// without the verifier only this client holds. Failing on a mismatch would let anyone who can reach the port
    /// break our own sign-in.
    /// </summary>
    [Test]
    public async Task LoginWithDiscordAsync_WhenTheCallbackStateDoesNotMatch_StillSignsInFromThePoll() {

        // Arrange
        ArrangeStart("session-1");
        ArrangePoll(SignedIn());
        _listenerFactory.Listener!.CallsBackWith("session-from-somewhere-else");

        // Act
        User user = await CreateService().LoginWithDiscordAsync();

        // Assert
        Assert.That(user.UserId, Is.EqualTo("bg-1"), "A stray callback should be ignored, not treated as a failure");

    }

    [Test]
    public async Task LoginWithDiscordAsync_OnSuccess_DisposesTheListener() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        FakeLoopbackAuthListener listener = _listenerFactory.Listener!;

        // Act
        await CreateService().LoginWithDiscordAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(listener.WasDisposed, Is.True, "The listener holds a bound port and must not outlive the sign-in");
            Assert.That(listener.WaitWasCancelled, Is.True, "The accept loop should be released before disposal");
        }

    }

    [Test]
    public void LoginWithDiscordAsync_WhenTheSignInIsRefused_ThrowsWithTheApisDescriptionAndDisposesTheListener() {

        // Arrange
        ArrangeStart();
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.Failed, null, "Auth.Discord.Cancelled", "You cancelled the Discord sign-in."));
        FakeLoopbackAuthListener listener = _listenerFactory.Listener!;
        UserService service = CreateService();

        // Act
        InvalidOperationException? thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoginWithDiscordAsync());

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(thrown!.Message, Is.EqualTo("You cancelled the Discord sign-in."), "The API's description is what the login view shows the user");
            Assert.That(listener.WasDisposed, Is.True, "The listener must be released on the failure path too");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "A refused sign-in should leave no credentials on disk");
        }

    }

    [Test]
    public void LoginWithDiscordAsync_WhenAMergeIsRequired_ThrowsAndWritesNoTokenStore() {

        // Arrange
        ArrangeStart();
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.MergeRequired));
        UserService service = CreateService();

        // Act
        InvalidOperationException? thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoginWithDiscordAsync());

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(thrown!.Message, Does.Contain("website"), "The client has no merge UI and should point the user at the one that does");
            Assert.That(File.Exists(_tokenStorePath), Is.False, "A merge offer carries no tokens to store");
        }

    }

    [Test]
    public void LoginWithDiscordAsync_WhenTheSessionTimesOut_ThrowsAskingTheUserToRetry() {

        // Arrange
        ArrangeStart();
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.TimedOut));
        UserService service = CreateService();

        // Act
        InvalidOperationException? thrown = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoginWithDiscordAsync());

        // Assert
        Assert.That(thrown!.Message, Does.Contain("try again"), "A timeout is recoverable and should read that way");

    }

    /// <summary>
    /// A cancel has to reach the poll, or the sign-in the user abandoned keeps running for its whole budget.
    /// </summary>
    [Test]
    public void LoginWithDiscordAsync_WhenCancelled_CancelsThePollAndDisposesTheListener() {

        // Arrange
        ArrangeStart();
        CancellationToken pollToken = default;
        _webAPI.EndAuthAsync(Arg.Any<AuthProvider>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Do<CancellationToken>(token => pollToken = token))
            .Returns(new AuthStatusResult(AuthStatusOutcome.Cancelled));
        FakeLoopbackAuthListener listener = _listenerFactory.Listener!;
        UserService service = CreateService();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Assert.ThrowsAsync<OperationCanceledException>(async () => await service.LoginWithDiscordAsync(cts.Token));

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(pollToken.IsCancellationRequested, Is.True, "The caller's cancellation should reach the poll");
            Assert.That(listener.WasDisposed, Is.True, "The listener must be released when the sign-in is abandoned");
        }

    }

    /// <summary>
    /// Cancelling was purely local until this: the poll stopped and the listener closed, but the session stayed valid
    /// for its five minutes, so a user who then finished in the browser had tokens minted for a client that had
    /// stopped listening.
    /// </summary>
    [Test]
    public void LoginWithDiscordAsync_WhenCancelled_AbandonsTheSessionServerSide() {

        // Arrange
        ArrangeStart("session-7");
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.Cancelled));
        UserService service = CreateService();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Assert.ThrowsAsync<OperationCanceledException>(async () => await service.LoginWithDiscordAsync(cts.Token));

        // Assert
        _webAPI.Received(1).CancelAuthAsync("session-7", "verifier-1");

    }

    /// <summary>
    /// The verifier is what proves this client started the session, and without it the API ignores the request -- so
    /// an abandoned session would quietly stay alive while everything here looked like it had worked.
    /// </summary>
    [Test]
    public async Task LoginWithDiscordAsync_WhenTheSignInSucceeds_DoesNotAbandonTheSession() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        UserService service = CreateService();

        // Act
        await service.LoginWithDiscordAsync();

        // Assert
        await _webAPI.DidNotReceive().CancelAuthAsync(Arg.Any<string>(), Arg.Any<string>());

    }

    /// <summary>
    /// The user is looking at a browser and the outcome is in the launcher, so the launcher asks for their attention.
    /// </summary>
    [Test]
    public async Task LoginWithDiscordAsync_WhenTheSignInResolves_AsksForTheUsersAttention() {

        // Arrange
        ArrangeStart();
        ArrangePoll(SignedIn());
        UserService service = CreateService();

        // Act
        await service.LoginWithDiscordAsync();

        // Assert
        _windowActivation.Received(1).Activate();

    }

    /// <summary>
    /// A refusal is worth surfacing too -- the message explaining it is in the launcher, behind the browser.
    /// </summary>
    [Test]
    public void LoginWithDiscordAsync_WhenTheSignInIsRefused_AsksForTheUsersAttention() {

        // Arrange
        ArrangeStart();
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.Failed, null, "Auth.Discord.Cancelled", "You cancelled the Discord sign-in."));
        UserService service = CreateService();

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoginWithDiscordAsync());

        // Assert
        _windowActivation.Received(1).Activate();

    }

    /// <summary>
    /// Except when the user cancelled it themselves, in which case they pressed the button and are already here.
    /// </summary>
    [Test]
    public void LoginWithDiscordAsync_WhenCancelled_LeavesTheWindowAlone() {

        // Arrange
        ArrangeStart();
        ArrangePoll(new AuthStatusResult(AuthStatusOutcome.Cancelled));
        UserService service = CreateService();

        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        Assert.ThrowsAsync<OperationCanceledException>(async () => await service.LoginWithDiscordAsync(cts.Token));

        // Assert
        _windowActivation.DidNotReceive().Activate();

    }

    [Test]
    public void LoginWithDiscordAsync_WhenTheSessionCannotBeStarted_Throws() {

        // Arrange
        _webAPI.StartAuthAsync(Arg.Any<AuthProvider>(), Arg.Any<string?>()).Returns((StartAuthResponse?)null);
        FakeLoopbackAuthListener listener = _listenerFactory.Listener!;
        UserService service = CreateService();

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.LoginWithDiscordAsync());

        // Assert
        Assert.That(listener.WasDisposed, Is.True, "The listener must be released even when the session never started");

    }

}

/// <summary>
/// A hand-written <see cref="ILoopbackAuthListenerFactory"/>, following the <see cref="MockIntegrationUserService"/>
/// precedent rather than a substitute: the tests need to control when the callback resolves and to observe disposal,
/// which reads more clearly as a fake than as a stack of NSubstitute setups.
/// </summary>
internal sealed class FakeLoopbackAuthListenerFactory : ILoopbackAuthListenerFactory {

    public FakeLoopbackAuthListener? Listener { get; set; } = new();

    /// <summary>
    /// The address the listener reports. Defaults to something shaped like a real one.
    /// </summary>
    public string ReturnUrl {
        get => Listener?.ReturnUrl ?? string.Empty;
        set {
            if (Listener is not null) {
                Listener.ReturnUrl = value;
            }
        }
    }

    public ILoopbackAuthListener? TryStart() => Listener;

}

/// <inheritdoc cref="FakeLoopbackAuthListenerFactory"/>
internal sealed class FakeLoopbackAuthListener : ILoopbackAuthListener {

    private readonly TaskCompletionSource<string?> _callback = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string ReturnUrl { get; set; } = "http://127.0.0.1:50000/auth/callback";

    public bool WasDisposed { get; private set; }

    public bool WaitWasCancelled { get; private set; }

    /// <summary>
    /// Resolves the wait with a state, as a browser following the redirect would.
    /// </summary>
    public void CallsBackWith(string state) => _callback.TrySetResult(state);

    /// <summary>
    /// Leaves the wait outstanding, as a silently rejected return URL does.
    /// </summary>
    public void NeverCallsBack() { }

    public async Task<string?> WaitForCallbackAsync(CancellationToken cancellationToken) {
        using CancellationTokenRegistration registration = cancellationToken.Register(() => {
            WaitWasCancelled = true;
            _callback.TrySetResult(null);   // Null on cancellation, never a fault -- the interface contract.
        });
        return await _callback.Task;
    }

    public void Dispose() => WasDisposed = true;

}
