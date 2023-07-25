namespace Battlegrounds.Game.Scenarios;

/// <summary>
/// The theatre of war a scenario is taking place in.
/// </summary>
public enum ScenarioTheatre {

    /// <summary>
    /// Axis vs Soviets
    /// </summary>
    EasternFront,

    /// <summary>
    /// Axis vs UKF & USF
    /// </summary>
    WesternFront,

    /// <summary>
    /// Axis vs Allies (Germany)
    /// </summary>
    SharedFront,

    /// <summary>
    /// Axis vs UKF/USF
    /// </summary>
    ItalianFront,

    /// <summary>
    /// The Mediterranean front => as of right now; Italian front
    /// </summary>
    MediterraneanFront = ItalianFront,

    /// <summary>
    /// Axis vs UKF/USF
    /// </summary>
    AfricanFront,

}
