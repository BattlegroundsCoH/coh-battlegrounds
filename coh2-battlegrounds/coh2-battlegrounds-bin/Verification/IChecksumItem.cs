namespace Battlegrounds.Verification {

    /// <summary>
    /// Interface for calculating a checksum for a C# data object.
    /// </summary>
    public interface IChecksumItem : IChecksumPropertyItem {

        /// <summary>
        /// Verify the checksum through internal checksum value.
        /// </summary>
        /// <remarks>
        /// Consider using the <see cref="IChecksumItem.VerifyChecksum(string)"/> method instead.
        /// </remarks>
        /// <returns><see langword="true"/> if the checksum is valid; Otherwise <see langword="false"/>.</returns>
        public bool VerifyChecksum();

        /// <summary>
        /// Verify the checksum of the object by generating the checksum and comparing with <paramref name="checksum"/>.
        /// </summary>
        /// <param name="checksum">The checksum value to compare against.</param>
        /// <returns><see langword="true"/> if the checksum is valid; Otherwise <see langword="false"/>.</returns>
        public bool VerifyChecksum(string checksum);

        /// <summary>
        /// Trigger a recalculation of the checksum value of the object.
        /// </summary>
        void CalculateChecksum();

    }

}
