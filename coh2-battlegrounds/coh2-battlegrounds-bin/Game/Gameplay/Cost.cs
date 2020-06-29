using Battlegrounds.Game.Database.json;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// Represents a cost using the three CoH2 resources and an additional field time. Implements <see cref="IJsonObject"/>
    /// </summary>
    public class Cost : IJsonObject {

        /// <summary>
        /// Manpower cost of the <see cref="Cost"/> data.
        /// </summary>
        public ushort Manpower { get; set; }

        /// <summary>
        /// Munitions cost of the <see cref="Cost"/> data.
        /// </summary>
        public ushort Munitions { get; set; }

        /// <summary>
        /// Fuel cost of the <see cref="Cost"/> data.
        /// </summary>
        public ushort Fuel { get; set; }

        /// <summary>
        /// The amount of time it takes to field something.
        /// </summary>
        public double FieldTime { get; set; }

        /// <summary>
        /// Default values for a new instance of the <see cref="Cost"/> data.
        /// </summary>
        public Cost() {
            this.Manpower = 0;
            this.Munitions = 0;
            this.Fuel = 0;
            this.FieldTime = -1.0;
        }

        /// <summary>
        /// New <see cref="Cost"/> data instance with resource values.
        /// </summary>
        /// <param name="man">The manpower value</param>
        /// <param name="mun">The munitions value</param>
        /// <param name="ful">The fuel value</param>
        public Cost(ushort man, ushort mun, ushort ful) : this() {
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
        public Cost(ushort man, ushort mun, ushort ful, double fti) : this(man, mun, ful) {
            this.FieldTime = fti;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => (this.FieldTime != -1.0) ? 
            $"{{Manpower = {this.Manpower}, Munitions = {this.Munitions}, Fuel = {this.Fuel}, FieldTime = {this.FieldTime}s}}" : 
            $"{{Manpower = {this.Manpower}, Munitions = {this.Munitions}, Fuel = {this.Fuel}}}";

    }

}
