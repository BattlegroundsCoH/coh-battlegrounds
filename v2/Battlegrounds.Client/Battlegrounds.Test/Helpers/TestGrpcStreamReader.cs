using System.Threading.Channels;

using Battlegrounds.Proto.Lobbies;

using Grpc.Core;

namespace Battlegrounds.Test.Helpers;

/// <summary>
/// An <see cref="IAsyncStreamReader{T}"/> implementation backed by an in-memory
/// <see cref="Channel{T}"/>. Push messages via <see cref="PushAsync"/> and signal
/// end-of-stream via <see cref="Complete"/>.
/// </summary>
/// <remarks>
/// Wrap in an <see cref="AsyncServerStreamingCall{TResponse}"/> for construction of a
/// <see cref="Battlegrounds.Models.Lobbies.MultiplayerLobby"/> under test:
/// <code>
/// var reader = new TestGrpcStreamReader();
/// var call = TestGrpcStreamReader.WrapInServerStreamingCall(reader);
/// var lobby = new MultiplayerLobby("lobby-1", call, grpcClientMock, setup, ...);
/// </code>
/// </remarks>
public sealed class TestGrpcStreamReader : IAsyncStreamReader<LobbyStateUpdate> {

    private readonly Channel<LobbyStateUpdate> _channel =
        Channel.CreateUnbounded<LobbyStateUpdate>(new UnboundedChannelOptions { SingleReader = true });

    private LobbyStateUpdate _current = new();

    /// <inheritdoc/>
    public LobbyStateUpdate Current => _current;

    /// <summary>Enqueues a <see cref="LobbyStateUpdate"/> to be returned by the next <see cref="MoveNext"/> call.</summary>
    public ValueTask PushAsync(LobbyStateUpdate update) =>
        _channel.Writer.WriteAsync(update);

    /// <summary>Signals end-of-stream; subsequent <see cref="MoveNext"/> calls will return <see langword="false"/>.</summary>
    public void Complete() => _channel.Writer.TryComplete();

    /// <inheritdoc/>
    public async Task<bool> MoveNext(CancellationToken cancellationToken = default) {
        if (await _channel.Reader.WaitToReadAsync(cancellationToken)) {
            _current = await _channel.Reader.ReadAsync(cancellationToken);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Wraps this reader in a <see cref="AsyncServerStreamingCall{TResponse}"/> suitable for
    /// passing to the <see cref="Battlegrounds.Models.Lobbies.MultiplayerLobby"/> constructor.
    /// </summary>
    public AsyncServerStreamingCall<LobbyStateUpdate> WrapInCall() =>
        new(
            responseStream: this,
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => Status.DefaultSuccess,
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { });
}
