using Battlegrounds.Core.Games;
using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobbySlot : ILobbySlot {

    private bool _isVisible;
    private bool _isLocked;
    private GrpcLobbyPlayer? _player;
    private AIDifficulty _difficulty = AIDifficulty.AI_HUMAN;

    public bool IsVisible => _isVisible;

    public bool IsLocked => _isLocked;

    public ILobbyPlayer? Player => _player;

    public AIDifficulty Difficulty => _difficulty;

    public void FromProto(LobbySlot lobbySlot) {
        _isLocked = lobbySlot.State == LobbySlotState.StateLocked;
        _isVisible = lobbySlot.State != LobbySlotState.StateUnavailable;
        _difficulty = AIDifficulty.FromInt((int)lobbySlot.AiLevel);
        if (lobbySlot.HasUserId || lobbySlot.AiLevel != LobbySlotAIState.AiHuman) {
            _player = _player?.FromProto(lobbySlot) ?? new GrpcLobbyPlayer().FromProto(lobbySlot);
        } else {
            _player = null;
        }
    }

}
