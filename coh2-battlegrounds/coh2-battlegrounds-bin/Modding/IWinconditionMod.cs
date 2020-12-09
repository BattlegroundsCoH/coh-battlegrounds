namespace Battlegrounds.Modding {

    /// <summary>
    /// Readonly struct representing an option available in a <see cref="IWinconditionMod"/>.
    /// </summary>
    public readonly struct WinconditionOption {
        
        /// <summary>
        /// The display title of the option.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The backing value of the option.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Initialize a new <see cref="WinconditionOption"/> with a display name and backing value.
        /// </summary>
        /// <param name="title">The name to display when picking the option.</param>
        /// <param name="val">The integer backing value that is used to represent the option in code.</param>
        public WinconditionOption(string title, int val) {
            this.Title = title;
            this.Value = val;
        }

        public void Deconstruct(out string title, out int value) {
            title = this.Title;
            value = this.Value;
        }

        public override string ToString() => this.Title;

    }

    /// <summary>
    /// Interface for representing a win condition mod.
    /// </summary>
    public interface IWinconditionMod : IGameMod {

        /// <summary>
        /// The options available for the mod.
        /// </summary>
        public WinconditionOption[] Options { get; set; }

        /// <summary>
        /// The index of the default <see cref="WinconditionOption"/>.
        /// </summary>
        public int DefaultOptionIndex { get; set; }

    }

}
