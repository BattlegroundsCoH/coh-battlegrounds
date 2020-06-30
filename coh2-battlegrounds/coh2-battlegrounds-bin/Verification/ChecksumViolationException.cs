using System;

namespace Battlegrounds.Verification {

    /// <summary>
    /// <see cref="Exception"/> thrown when a <see cref="IChecksumItem"/> failed to verify its checksum.
    /// </summary>
    public class ChecksumViolationException : Exception {

        /// <summary>
        /// New <see cref="ChecksumViolationException"/> instance.
        /// </summary>
        public ChecksumViolationException() : base("Checksum verification error!") { }

    }

}
