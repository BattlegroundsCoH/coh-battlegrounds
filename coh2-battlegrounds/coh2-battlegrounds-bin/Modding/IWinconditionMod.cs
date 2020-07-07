namespace Battlegrounds.Modding {

    public readonly struct WinconditionOption {
        public string Title { get; }
        public int Value { get; }
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
