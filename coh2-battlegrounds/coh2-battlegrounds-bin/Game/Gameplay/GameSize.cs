namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// Struct representing a size in the game along the X and Y axis
/// </summary>
public struct GameSize {

    /// <summary>
    /// The game size of (0, 0)
    /// </summary>
    public static readonly GameSize Naught = new GameSize(0.0f, 0.0f);

    private double m_w;
    private double m_l;

    /// <summary>
    /// Get or set the width component.
    /// </summary>
    public double Width {
        get => this.m_w;
        set => this.m_w = value;
    }

    /// <summary>
    /// Get or set the length component.
    /// </summary>
    public double Length {
        get => this.m_l;
        set => this.m_l = value;
    }

    /// <summary>
    /// Initialise a new <see cref="GameSize"/> instance.
    /// </summary>
    /// <param name="w">The Width component</param>
    /// <param name="l">The Length component</param>
    public GameSize(double w, double l) : this() {
        this.m_w = w;
        this.m_l = l;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public (double w, double l) ToTuple2() => (this.Width, this.Length);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="w"></param>
    /// <param name="l"></param>
    public void Deconstruct(out double w, out double l) {
        w = this.m_w;
        l = this.m_l;
    }

    /// <summary>
    /// Get a new <see cref="GameSize"/> with swapped width and length coordinates.
    /// </summary>
    /// <returns>A copy with the width and length information swapped.</returns>
    public GameSize SwapWL() => this with { m_l = this.m_w, m_w = this.m_l };

}
