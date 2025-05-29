namespace Battlegrounds.Models.Lobbies;

public sealed record Participant(int LobbyId, string ParticipantId, string ParticipantName, bool IsAIParticipant);
