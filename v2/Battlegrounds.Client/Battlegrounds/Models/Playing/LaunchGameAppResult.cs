using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Models.Playing;

public sealed class LaunchGameAppResult {

    [MemberNotNullWhen(false, nameof(GameInstance))]
    public bool Failed { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public GameAppInstance? GameInstance { get; init; }

}
