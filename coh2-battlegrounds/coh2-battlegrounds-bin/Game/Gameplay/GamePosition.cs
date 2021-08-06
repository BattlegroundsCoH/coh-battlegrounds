namespace Battlegrounds.Game {

    /// <summary>
    /// Represents a world position in the game using X,Y,Z variables
    /// </summary>
    public struct GamePosition {

        private float[] _coords;

        /// <summary>
        /// The X-coordinate
        /// </summary>
        public float X { get => _coords[0]; set => _coords[0] = value; }

        /// <summary>
        /// The Y-coordinate
        /// </summary>
        public float Y { get => _coords[1]; set => _coords[1] = value; }

        /// <summary>
        /// The Z-coordinate
        /// </summary>
        public float Z { get => _coords[2]; set => _coords[2] = value; }

        /// <summary>
        /// New instance of a <see cref="GamePosition"/> using only the two first coordinates (XY) - a 2D position
        /// </summary>
        /// <param name="x">The X-coordinate</param>
        /// <param name="y">The Y-coordinate</param>
        public GamePosition(float x, float y) {
            this._coords = new float[3];
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// New instance of a <see cref="GamePosition"/> using all three coordinates (XYZ) - a 3D position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public GamePosition(float x, float y, float z) : this(x, y) {
            this._coords[2] = z;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"({X}, {Y}, {Z})";

        /// <summary>
        /// The game position at (0, 0, 0)
        /// </summary>
        public static GamePosition Naught => new GamePosition(0.0f, 0.0f, 0.0f);

    }

}
