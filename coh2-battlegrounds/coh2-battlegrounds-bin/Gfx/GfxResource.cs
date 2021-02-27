namespace Battlegrounds.Gfx {
    
    /// <summary>
    /// Represents a GFX resource that can be opened into a <see cref="GfxResourceStream"/> for reading.
    /// </summary>
    public class GfxResource {

        private string m_id;
        private int m_width, m_height;
        private byte[] m_raw;

        /// <summary>
        /// Initialize a new <see cref="GfxResource"/> class with raw data.
        /// </summary>
        /// <param name="id">The identifier to use when identifying the resource.</param>
        /// <param name="rawData">The raw byte data of the GFX resource</param>
        /// <param name="w">The width of the resource.</param>
        /// <param name="h">The height of the resource.</param>
        public GfxResource(string id, byte[] rawData, double w, double h) {
            this.m_id = id;
            this.m_raw = rawData;
            this.m_width = (int)w;
            this.m_height = (int)h;
        }

        /// <summary>
        /// Verify if the <see cref="GfxResource"/> is has identifier matching the given parameter.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>Will return <see langword="true"/> if identifier is matching resource identifier. Otherwise <see langword="false"/>.</returns>
        public bool IsResource(string identifier) => this.m_id == identifier;

        /// <summary>
        /// Opens a new <see cref="GfxResourceStream"/> for reading the data contained within the <see cref="GfxResource"/>.
        /// </summary>
        /// <returns>An open <see cref="GfxResourceStream"/>.</returns>
        public GfxResourceStream Open() {
            return new GfxResourceStream(this.m_id, this.m_raw, this.m_width, this.m_height) {
                Position = 0
            };
        }

    }

}
