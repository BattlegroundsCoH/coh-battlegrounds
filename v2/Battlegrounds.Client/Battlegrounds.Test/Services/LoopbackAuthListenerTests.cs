using System.Net;
using System.Net.Sockets;
using System.Text;

using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.Services.Infrastructure;

namespace Battlegrounds.Test.Services;

/// <summary>
/// Covers the local listener a browser sign-in returns to.
/// </summary>
/// <remarks>These bind a loopback socket in-process rather than mocking one. That is hermetic -- no container, no
/// network, no fixed port -- so they are deliberately <i>not</i> tagged as integration tests and must keep running in
/// CI, which filters those out. The listener is the one place where getting the address subtly wrong fails silently
/// in production, so it is worth exercising for real.</remarks>
[TestOf(typeof(LoopbackAuthListenerFactory))]
public class LoopbackAuthListenerTests {

    /// <summary>
    /// The one entry the deployed API allows. Every part of a return URL but the port has to match it.
    /// </summary>
    private const string DeployedAllowlistEntry = "http://127.0.0.1/auth/callback";

    private TestLogger<LoopbackAuthListenerFactory> _logger;
    private Configuration _configuration;
    private readonly List<ILoopbackAuthListener> _listeners = [];

    [SetUp]
    public void SetUp() {
        _logger = new TestLogger<LoopbackAuthListenerFactory>();
        _configuration = new Configuration();
    }

    [TearDown]
    public void TearDown() {
        foreach (ILoopbackAuthListener listener in _listeners) {
            listener.Dispose();
        }
        _listeners.Clear();
        _logger.Dispose();
    }

    /// <summary>
    /// Starts a listener and registers it for disposal, so a failing test cannot leak a bound port.
    /// </summary>
    private ILoopbackAuthListener StartListener(TimeSpan? connectionTimeout = null) {
        LoopbackAuthListenerFactory factory = new(_logger, _configuration, connectionTimeout);
        ILoopbackAuthListener? listener = factory.TryStart();
        Assert.That(listener, Is.Not.Null, "A loopback listener should have started");
        _listeners.Add(listener);
        return listener;
    }

    private static HttpClient NewBrowser() => new() { Timeout = TimeSpan.FromSeconds(10) };

    [Test]
    public void TryStart_WhenLoopbackIsAvailable_BindsAnEphemeralPort() {

        // Act
        ILoopbackAuthListener listener = StartListener();

        // Assert
        Uri returnUrl = new(listener.ReturnUrl);
        using (Assert.EnterMultipleScope()) {
            Assert.That(returnUrl.Port, Is.Not.Zero, "The listener should report the port it actually bound, not 0");
            Assert.That(returnUrl.Port, Is.GreaterThan(1024), "An ephemeral port should be above the well-known range");
        }

    }

    /// <summary>
    /// The API compares scheme, host and path exactly and lets only the port float, and only because its allowlist
    /// entry is a loopback literal. A return URL that misses is <i>ignored rather than refused</i>, so this is the
    /// one mistake that would ship silently -- pinning it here is what stops a well-meaning move to
    /// <c>localhost</c> or a trailing slash on the path from reaching production.
    /// </summary>
    [Test]
    public void ReturnUrl_MatchesTheDeployedAllowlistEntryOnEveryPartButThePort() {

        // Arrange
        Uri allowed = new(DeployedAllowlistEntry);

        // Act
        Uri returnUrl = new(StartListener().ReturnUrl);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(returnUrl.Scheme, Is.EqualTo(allowed.Scheme), "The scheme has to match the allowlist entry");
            Assert.That(returnUrl.Host, Is.EqualTo(allowed.Host).IgnoreCase, "The host has to match the allowlist entry");
            Assert.That(returnUrl.AbsolutePath, Is.EqualTo(allowed.AbsolutePath), "The path is compared ordinally by the API and has to match exactly");
            Assert.That(returnUrl.HostNameType, Is.EqualTo(UriHostNameType.IPv4), "Only an IP literal is granted port latitude; a name such as localhost is not");
        }

    }

    [Test]
    public void TryStart_WhenLoopbackSignInIsDisabled_ReturnsNull() {

        // Arrange
        _configuration.API.UseLoopbackSignIn = false;
        LoopbackAuthListenerFactory factory = new(_logger, _configuration);

        // Act
        ILoopbackAuthListener? listener = factory.TryStart();

        // Assert
        Assert.That(listener, Is.Null, "Switching the loopback off should leave the caller to sign in without a return URL");

    }

    [Test]
    public async Task WaitForCallbackAsync_WhenTheBrowserFollowsTheRedirect_ReturnsTheState() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={state}");
        string body = await response.Content.ReadAsStringAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(await callback, Is.EqualTo(state), "The state carried by the redirect should be reported back");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The browser should be served a page, not an error");
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"), "The response should be an HTML page");
            Assert.That(body, Does.Contain("launcher"), "The page should tell the user to return to the launcher");
        }

    }

    /// <summary>
    /// Chromium asks for a favicon even where the page declares an inline one, and that request lands on this
    /// socket. Treating the first request as the callback would end the wait on it.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_WhenTheBrowserAlsoAsksForFavicon_IgnoresItAndKeepsWaiting() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage favicon = await browser.GetAsync($"http://127.0.0.1:{new Uri(listener.ReturnUrl).Port}/favicon.ico");
        HttpResponseMessage callbackResponse = await browser.GetAsync($"{listener.ReturnUrl}?state={state}");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(favicon.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "A request for anything but the callback path should be refused");
            Assert.That(callbackResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The callback that follows should still be served");
            Assert.That(await callback, Is.EqualTo(state), "The wait should have survived the favicon request");
        }

    }

    [Test]
    public async Task WaitForCallbackAsync_WhenTheCallbackCarriesNoState_KeepsWaiting() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage bare = await browser.GetAsync(listener.ReturnUrl);
        await browser.GetAsync($"{listener.ReturnUrl}?state={state}");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(bare.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "A callback with no state cannot identify a login session");
            Assert.That(await callback, Is.EqualTo(state), "The wait should have survived the stateless request");
        }

    }

    [Test]
    public async Task WaitForCallbackAsync_WhenTheStateIsNotAGuid_KeepsWaiting() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage nonsense = await browser.GetAsync($"{listener.ReturnUrl}?state=not-a-login-session");
        await browser.GetAsync($"{listener.ReturnUrl}?state={state}");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(nonsense.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "A state that is not a login session id should be refused");
            Assert.That(await callback, Is.EqualTo(state), "The wait should have survived the malformed request");
        }

    }

    /// <summary>
    /// Connections are served one at a time, so a peer that opens a socket and then says nothing would hold the
    /// accept loop forever without the per-connection timeout.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_WhenAConnectionSendsNothing_StillServesTheNextOne() {

        // Arrange
        ILoopbackAuthListener listener = StartListener(connectionTimeout: TimeSpan.FromMilliseconds(200));
        int port = new Uri(listener.ReturnUrl).Port;
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using TcpClient silent = new();
        await silent.ConnectAsync(IPAddress.Loopback, port);

        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={state}");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(await callback, Is.EqualTo(state), "A silent connection should be dropped rather than wedging the accept loop");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The real callback should still be served");
        }

    }

    [Test]
    public async Task WaitForCallbackAsync_WhenCancelled_ReturnsNullWithoutThrowing() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new();
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        await cts.CancelAsync();

        // Assert
        Assert.That(await callback, Is.Null, "Cancelling should end the wait quietly; the caller treats a fault here as unrecoverable");

    }

    [Test]
    public void Dispose_ReleasesThePort() {

        // Arrange
        LoopbackAuthListenerFactory factory = new(_logger, _configuration);
        ILoopbackAuthListener listener = factory.TryStart()!;
        int port = new Uri(listener.ReturnUrl).Port;

        // Act
        listener.Dispose();

        // Assert
        TcpListener rebind = new(IPAddress.Loopback, port);
        Assert.DoesNotThrow(() => rebind.Start(), "Disposing should release the port for the next sign-in attempt");
        rebind.Dispose();

    }

    [Test]
    public void Dispose_WhenCalledTwice_DoesNotThrow() {

        // Arrange
        LoopbackAuthListenerFactory factory = new(_logger, _configuration);
        ILoopbackAuthListener listener = factory.TryStart()!;

        // Act
        listener.Dispose();

        // Assert
        Assert.DoesNotThrow(listener.Dispose, "The caller disposes through a using block that may already have run");

    }

    /// <summary>
    /// The pages are authored as HTML files and embedded by the build. If that embedding breaks -- a renamed file, a
    /// dropped project item -- the listener falls back to a minimal page rather than throwing, so the sign-in keeps
    /// working and nothing surfaces. This is what notices.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_ServesThePageFromTheEmbeddedHtmlFileRatherThanTheFallback() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={Guid.NewGuid()}");
        string body = await response.Content.ReadAsStringAsync();
        await callback;

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(body, Does.Contain("class=\"glow\""), "The authored page should be served; the fallback carries no markup of its own");
            Assert.That(body, Does.Contain("rel=\"icon\" href=\"data:,\""), "The inline icon is what stops Chromium asking for /favicon.ico");
            Assert.That(body, Does.Contain("#e0a53b"), "The page should carry the design system's gold accent");
        }

    }

    /// <summary>
    /// The page has to carry all three outcomes, because the listener serves the same bytes for every one of them and
    /// lets the page pick from the query string it was fetched with.
    /// </summary>
    /// <remarks>Dropping the switch is the regression this catches, and it would not otherwise surface: the page still
    /// renders, still says "Signed in", and a cancelled sign-in goes back to contradicting the launcher exactly as it
    /// did before the marker existed.</remarks>
    [Test]
    public async Task WaitForCallbackAsync_ServesAPageThatCanRenderEveryOutcome() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={Guid.NewGuid()}&status=cancelled");
        string body = await response.Content.ReadAsStringAsync();
        await callback;

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(body, Does.Contain("Sign-in cancelled"), "The cancelled wording should be in the page the listener serves");
            Assert.That(body, Does.Contain("Signed in"), "The signed-in wording is the default state and should still be there");
            Assert.That(body, Does.Contain("location.search"), "The page picks its state from the query the API redirected with");
            Assert.That(body, Does.Not.Contain("innerHTML"), "The code on the redirect is untrusted text and must be written as text");
        }

    }

    /// <summary>
    /// A refusal reaches the listener looking exactly like a success -- a well-formed state on the callback path -- so
    /// it must be reported as one. The outcome is the page's business, not the listener's.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_ReportsACancelledSignInLikeAnyOther() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);
        string state = Guid.NewGuid().ToString();

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync(
            $"{listener.ReturnUrl}?state={state}&status=cancelled&code=Auth.Discord.Cancelled");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(await callback, Is.EqualTo(state), "The state should be reported whatever the outcome was");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "A refusal is still a callback that arrived");
        }

    }

    /// <summary>
    /// The listener answers one path, so a logo referenced by URL would be a request it 404s. It has to travel in
    /// the page itself.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_InlinesTheLogoRatherThanReferencingIt() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={Guid.NewGuid()}");
        string body = await response.Content.ReadAsStringAsync();
        await callback;

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(body, Does.Contain("src=\"data:image/svg+xml;base64,"), "The logo should be inlined as a data URI");
            Assert.That(body, Does.Not.Contain("src=\"/"), "Nothing on the page may be fetched from this socket, which serves the callback path alone");
        }

    }

    /// <summary>
    /// The page is UTF-8 and says so in its own meta tag, but the bytes on the wire are what the browser reads
    /// first. The em dash in the title is the character that shows this went wrong.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_ServesThePageAsUtf8() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using HttpClient browser = NewBrowser();
        HttpResponseMessage response = await browser.GetAsync($"{listener.ReturnUrl}?state={Guid.NewGuid()}");
        byte[] raw = await response.Content.ReadAsByteArrayAsync();
        await callback;

        // Assert
        string decoded = Encoding.UTF8.GetString(raw);
        using (Assert.EnterMultipleScope()) {
            Assert.That(response.Content.Headers.ContentType?.CharSet, Is.EqualTo("utf-8").IgnoreCase, "The charset should be declared on the response");
            Assert.That(decoded, Does.Contain("Signed in — Company of Heroes: Battlegrounds"), "The em dash should survive the round trip intact");
            Assert.That(raw.Length, Is.EqualTo(Encoding.UTF8.GetByteCount(decoded)), "Content-Length is in bytes, so a multi-byte character must not be counted as one");
        }

    }

    /// <summary>
    /// The response has to be readable by a browser that speaks HTTP/1.1, which means a status line, a content
    /// length and a body that matches it. A hand-written response is the cost of not using http.sys, so it is worth
    /// asserting on the bytes rather than only on what <see cref="HttpClient"/> makes of them.
    /// </summary>
    [Test]
    public async Task WaitForCallbackAsync_WhenServingTheCallback_WritesAWellFormedResponse() {

        // Arrange
        ILoopbackAuthListener listener = StartListener();
        int port = new Uri(listener.ReturnUrl).Port;
        string state = Guid.NewGuid().ToString();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        Task<string?> callback = listener.WaitForCallbackAsync(cts.Token);

        // Act
        using TcpClient client = new();
        await client.ConnectAsync(IPAddress.Loopback, port);
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(Encoding.ASCII.GetBytes($"GET /auth/callback?state={state} HTTP/1.1\r\nHost: 127.0.0.1:{port}\r\n\r\n"));

        using StreamReader reader = new(stream, Encoding.UTF8);
        string raw = await reader.ReadToEndAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(await callback, Is.EqualTo(state), "The state should be reported back");
            Assert.That(raw, Does.StartWith("HTTP/1.1 200 OK\r\n"), "The response should open with a status line");
            Assert.That(raw, Does.Contain("Content-Type: text/html; charset=utf-8\r\n"), "The browser needs to be told this is HTML");
            Assert.That(raw, Does.Contain("Connection: close\r\n"), "The connection is not reused and should say so");
            Assert.That(raw, Does.Contain($"Content-Length: {Encoding.UTF8.GetByteCount(raw[(raw.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4)..])}\r\n"), "The declared length should match the body actually written");
        }

    }

}
