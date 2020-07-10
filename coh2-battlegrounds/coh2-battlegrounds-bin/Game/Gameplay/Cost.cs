using Battlegrounds.Json;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// Represents a cost using the three CoH2 resources and an additional field time. Implements <see cref="IJsonObject"/>
    /// </summary>
    public class Cost : IJsonObject {

        /// <summary>
        /// Manpower cost of the <see cref="Cost"/> data.
        /// </summary>
        public float Manpower { get; set; }

        /// <summary>
        /// Munitions cost of the <see cref="Cost"/> data.
        /// </summary>
        public float Munitions { get; set; }

        /// <summary>
        /// Fuel cost of the <see cref="Cost"/> data.
        /// </summary>
        public float Fuel { get; set; }

        /// <summary>
        /// The amount of time it takes to field something.
        /// </summary>
        public float FieldTime { get; set; }

        /// <summary>
        /// Default values for a new instance of the <see cref="Cost"/> data.
        /// </summary>
        public Cost() {
            this.Manpower = 0;
            this.Munitions = 0;
            this.Fuel = 0;
            this.FieldTime = -1.0f;
        }

        /// <summary>
        /// New <see cref="Cost"/> data instance with resource values.
        /// </summary>
        /// <param name="man">The manpower value</param>
        /// <param name="mun">The munitions value</param>
        /// <param name="ful">The fuel value</param>
        public Cost(float man, float mun, float ful) : this() {
            this.Manpower = man;
            this.Munitions = mun;
            this.Fuel = ful;
        }

        /// <summary>
        /// New <see cref="Cost"/> data instance with resource values and field time.
        /// </summary>
        /// <param name="man">The manpower value</param>
        /// <param name="mun">The munitions value</param>
        /// <param name="ful">The fuel value</param>
        /// <param name="fti">The field time value</param>
        public Cost(float man, float mun, float ful, float fti) : this(man, mun, ful) {
            this.FieldTime = fti;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => (this.FieldTime != -1.0) ? 
            $"{{Manpower = {this.Manpower}, Munitions = {this.Munitions}, Fuel = {this.Fuel}, FieldTime = {this.FieldTime}s}}" : 
            $"{{Manpower = {this.Manpower}, Munitions = {this.Munitions}, Fuel = {this.Fuel}}}";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.ToString();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonReference"></param>
        public void FromJsonReference(string jsonReference) => throw new System.NotSupportedException();

        /// <summary>
        /// Add two costs together.
        /// </summary>
        /// <param name="a">Left operand</param>
        /// <param name="b">Right operand</param>
        /// <returns>The sum of the two operands.</returns>
        public static Cost operator +(Cost a, Cost b)
            => new Cost((float)(a.Manpower + b.Manpower), (float)(a.Munitions + b.Munitions), (float)(a.Fuel + b.Fuel), a.FieldTime + b.FieldTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Cost operator *(Cost a, float b)
            => new Cost((float)(a.Manpower * b), (float)(a.Munitions * b), (float)(a.Fuel * b), a.FieldTime * b);

    }

}
