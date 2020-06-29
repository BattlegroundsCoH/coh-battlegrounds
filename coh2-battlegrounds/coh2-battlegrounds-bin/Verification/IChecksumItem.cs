namespace Battlegrounds.Verification {

    /// <summary>
    /// Interface for calculating a checksum for a C# data object.
    /// </summary>
    public interface IChecksumItem {

        /// <summary>
        /// Get the complete checksum in string format.
        /// </summary>
        /// <returns>The string representation of the checksum.</returns>
        public string GetChecksum();

    }

}
