using Battlegrounds.Game.Scar;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// Represents a <see cref="Modifier"/> to apply to a <see cref="Squad"/>. Implements <see cref="IScarValue"/>.
    /// </summary>
    public readonly struct Modifier : IScarValue {

        /// <summary>
        /// The modifier value.
        /// </summary>
        public float Value { get; }

        /// <summary>
        /// The modifier name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create a new <see cref="Modifier"/>.
        /// </summary>
        /// <param name="modifiername">The name of the modifier to apply.</param>
        /// <param name="value">The value to modify by.</param>
        public Modifier(string modifiername, float value) {
            this.Value = value;
            this.Name = modifiername;
        }

        public override string ToString() => $"{this.Name} x{this.Value:0.00}";

        public string ToScar() => $"{{ name = \"{this.Name}\", value = {this.Value:0.00} }}";

    }

}
