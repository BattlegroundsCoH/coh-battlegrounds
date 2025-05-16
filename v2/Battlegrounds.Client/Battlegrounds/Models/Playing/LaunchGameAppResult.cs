using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Models.Playing;

public sealed class LaunchGameAppResult {

    [MemberNotNullWhen(false, nameof(GameInstance))]
    public bool Failed { get; }

    public GameAppInstance? GameInstance { get; }

}
