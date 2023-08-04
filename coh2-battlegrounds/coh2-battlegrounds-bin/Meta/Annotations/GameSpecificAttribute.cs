using System;

using Battlegrounds.Game;

namespace Battlegrounds.Meta.Annotations;

/// <summary>
/// An annotation for marking a feature as specific to a particular game.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | 
    AttributeTargets.Constructor | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Field)]
public sealed class GameSpecificAttribute : AnnotationAttribute {

    /// <summary>
    /// Gets the game targeted by this specific annotation.
    /// </summary>
    /// <value>The game.</value>
    public GameCase Game { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameSpecificAttribute"/> class
    /// with the specified game.
    /// </summary>
    /// <param name="game">The game targeted by this specific annotation.</param>
    public GameSpecificAttribute(GameCase game) {
        this.Game = game;
    }

}
