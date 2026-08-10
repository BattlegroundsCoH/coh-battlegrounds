using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Opens <see cref="LoopbackAuthListener"/> instances on an ephemeral loopback port.
/// </summary>
/// <param name="logger">The service logger instance.</param>
/// <param name="configuration">Supplies <see cref="Configuration.APIConfiguration.UseLoopbackSignIn"/>.</param>
/// <param name="connectionTimeout">How long a single connection may take to send its request line before it is
/// dropped. Supplied by tests so they do not wait out the default.</param>
public sealed class LoopbackAuthListenerFactory(
    ILogger<LoopbackAuthListenerFactory> logger,
    Configuration configuration,
    TimeSpan? connectionTimeout = null) : ILoopbackAuthListenerFactory {

    /// <summary>
    /// How many times to try for an ephemeral port before giving up and signing in without a return URL.
    /// </summary>
    private const int BindAttempts = 3;

    private static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(5);

    private readonly ILogger<LoopbackAuthListenerFactory> _logger = logger;
    private readonly Configuration _configuration = configuration;
    private readonly TimeSpan _connectionTimeout = connectionTimeout ?? DefaultConnectionTimeout;

    public ILoopbackAuthListener? TryStart() {

        if (!_configuration.API.UseLoopbackSignIn) {
            _logger.LogDebug("Loopback sign-in is switched off; the browser will end on the API's own success page.");
            return null;
        }

        for (int attempt = 1; attempt <= BindAttempts; attempt++) {
            TcpListener listener = new(IPAddress.Loopback, 0);
            try {
                listener.Start(backlog: 8);
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                _logger.LogDebug("Loopback sign-in listener bound to port {Port}.", port);
                return new LoopbackAuthListener(listener, port, _connectionTimeout, _logger);
            } catch (Exception ex) {
                listener.Dispose();
                _logger.LogWarning(ex, "Could not bind a loopback port for sign-in (attempt {Attempt} of {Attempts}).", attempt, BindAttempts);
            }
        }

        _logger.LogWarning("No loopback port could be bound; the sign-in result will be collected by polling alone.");
        return null;

    }

}

/// <summary>
/// A loopback socket serving the one redirect the API sends the browser at the end of a sign-in.
/// </summary>
/// <remarks>Speaks just enough HTTP for that: it reads a request line, ignores anything that is not a GET of
/// <see cref="CallbackPath"/>, and answers with a single self-contained page. Nothing here throws out to the caller
/// -- see <see cref="ILoopbackAuthListener.WaitForCallbackAsync"/> for why that matters.</remarks>
internal sealed class LoopbackAuthListener : ILoopbackAuthListener {

    /// <summary>
    /// The path the redirect arrives on.
    /// </summary>
    /// <remarks>Must match the API's <c>Auth:AllowedReturnUrls</c> entry byte for byte:
    /// <c>AuthReturnUrlOptions.TryResolve</c> compares <c>AbsolutePath</c> with
    /// <see cref="StringComparison.Ordinal"/>, and a return URL that does not match is ignored rather than refused --
    /// the sign-in still succeeds, this listener just never fires and nothing says why.</remarks>
    private const string CallbackPath = "/auth/callback";

    private const int MaxRequestLineBytes = 8 * 1024;

    private readonly TcpListener _listener;
    private readonly int _port;
    private readonly TimeSpan _connectionTimeout;
    private readonly ILogger _logger;

    private bool _disposed;

    internal LoopbackAuthListener(TcpListener listener, int port, TimeSpan connectionTimeout, ILogger logger) {
        _listener = listener;
        _port = port;
        _connectionTimeout = connectionTimeout;
        _logger = logger;
    }

    public string ReturnUrl => $"http://127.0.0.1:{_port}{CallbackPath}";

    public async Task<string?> WaitForCallbackAsync(CancellationToken cancellationToken) {

        while (!cancellationToken.IsCancellationRequested) {

            TcpClient client;
            try {
                client = await _listener.AcceptTcpClientAsync(cancellationToken);
            } catch (OperationCanceledException) {
                return null;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "The loopback sign-in listener stopped accepting connections.");
                return null;
            }

            if (await ServeAsync(client, cancellationToken) is { } state) {
                return state;
            }

        }

        return null;

    }

    /// <summary>
    /// Answers one connection.
    /// </summary>
    private async Task<string?> ServeAsync(TcpClient client, CancellationToken cancellationToken) {

        using (client) {

            using CancellationTokenSource connection = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            connection.CancelAfter(_connectionTimeout);

            try {

                NetworkStream stream = client.GetStream();

                string? requestLine = await ReadRequestLineAsync(stream, connection.Token);
                if (requestLine is null) {
                    return null;
                }

                string[] parts = requestLine.Split(' ');
                if (parts.Length is not 3 || !string.Equals(parts[0], "GET", StringComparison.Ordinal)) {
                    await RespondAsync(stream, 400, "Bad Request", NoticePage.Value, connection.Token);
                    return null;
                }

                string target = parts[1];
                int queryStart = target.IndexOf('?');
                string path = queryStart < 0 ? target : target[..queryStart];

                if (!string.Equals(path, CallbackPath, StringComparison.Ordinal)) {
                    await RespondAsync(stream, 404, "Not Found", NoticePage.Value, connection.Token);
                    return null;
                }

                string? state = queryStart < 0 ? null : TryReadState(target[(queryStart + 1)..]);
                if (state is null || !Guid.TryParse(state, out _)) {
                    _logger.LogWarning("A request reached the loopback callback without a usable login session state.");
                    await RespondAsync(stream, 400, "Bad Request", NoticePage.Value, connection.Token);
                    return null;
                }

                await RespondAsync(stream, 200, "OK", SignInPage.Value, connection.Token);
                return state;

            } catch (OperationCanceledException) {
                return null;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "A connection to the loopback sign-in listener could not be served.");
                return null;
            }

        }

    }

    private static async Task<string?> ReadRequestLineAsync(Stream stream, CancellationToken cancellationToken) {

        byte[] buffer = new byte[MaxRequestLineBytes];
        int filled = 0;

        while (filled < buffer.Length) {

            int read = await stream.ReadAsync(buffer.AsMemory(filled, buffer.Length - filled), cancellationToken);
            if (read is 0) {
                return null;
            }

            int newline = Array.IndexOf(buffer, (byte)'\n', filled, read);
            filled += read;

            if (newline >= 0) {
                int end = newline > 0 && buffer[newline - 1] is (byte)'\r' ? newline - 1 : newline;
                return Encoding.ASCII.GetString(buffer, 0, end);
            }

        }

        return null;

    }

    /// <summary>
    /// Pulls <c>state</c> out of a query string.
    /// </summary>
    /// <remarks>Hand-parsed rather than reached for through <c>System.Web</c>: there is exactly one key to find and
    /// the API escapes it with <see cref="Uri.EscapeDataString(string)"/>.</remarks>
    private static string? TryReadState(string query) {

        foreach (string pair in query.Split('&')) {
            int separator = pair.IndexOf('=');
            if (separator < 0 || !string.Equals(pair[..separator], "state", StringComparison.Ordinal)) {
                continue;
            }
            return Uri.UnescapeDataString(pair[(separator + 1)..]);
        }

        return null;

    }

    private static async Task RespondAsync(NetworkStream stream, int status, string reason, string body, CancellationToken cancellationToken) {

        byte[] payload = Encoding.UTF8.GetBytes(body);
        byte[] head = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status} {reason}\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            $"Content-Length: {payload.Length}\r\n" +
            "Cache-Control: no-store\r\n" +
            "Connection: close\r\n\r\n");

        await stream.WriteAsync(head, cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
        await stream.FlushAsync(cancellationToken);

        try {
            stream.Socket.Shutdown(SocketShutdown.Send);
        } catch (Exception) {
            // The peer may already be gone; the response is written either way.
        }

    }

    public void Dispose() {
        if (_disposed) {
            return;
        }
        _disposed = true;
        _listener.Dispose();
    }

    /// <summary>
    /// The page a resolved sign-in lands on, whatever the outcome. Authored as <c>Assets/Auth/signin.html</c>.
    /// </summary>
    private static readonly Lazy<string> SignInPage = new(() => LoadPage(
        "Battlegrounds.Auth.SignIn.html", "Signed in. You can close this tab and return to the Battlegrounds launcher."));

    /// <summary>
    /// The page anything else gets. Authored as <c>Assets/Auth/notice.html</c>.
    /// </summary>
    private static readonly Lazy<string> NoticePage = new(() => LoadPage(
        "Battlegrounds.Auth.Notice.html", "This address only serves the Battlegrounds sign-in callback."));

    /// <summary>
    /// Reads one of the listener's pages out of the assembly.
    /// </summary>
    /// <param name="resourceName">The <c>LogicalName</c> the project file gives the embedded page.</param>
    /// <param name="fallbackBody">Body text for the page built in place if the resource is missing.</param>
    private static string LoadPage(string resourceName, string fallbackBody) {

        using Stream? stream = typeof(LoopbackAuthListener).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null) {
            return $"""<!doctype html><html lang="en"><head><meta charset="utf-8"><title>Battlegrounds</title><link rel="icon" href="data:,"></head><body><p>{fallbackBody}</p></body></html>""";
        }

        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();

    }

}
