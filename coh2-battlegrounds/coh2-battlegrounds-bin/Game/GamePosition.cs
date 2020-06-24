using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public struct GamePosition {

        private float[] _coords;

        /// <summary>
        /// 
        /// </summary>
        public float X { get => _coords[0]; set => _coords[0] = value; }

        /// <summary>
        /// 
        /// </summary>
        public float Y { get => _coords[1]; set => _coords[1] = value; }

        /// <summary>
        /// 
        /// </summary>
        public float Z { get => _coords[2]; set => _coords[2] = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public GamePosition(float x, float y) {
            this._coords = new float[3];
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public GamePosition(float x, float y, float z) : this(x,y) {
            this._coords[2] = z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"({X}, {Y}, {Z})";

    }

}
