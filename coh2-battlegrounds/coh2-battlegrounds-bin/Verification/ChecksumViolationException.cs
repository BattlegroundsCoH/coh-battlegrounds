using System;
using System.Globalization;

namespace Battlegrounds.Verification {

    /// <summary>
    /// <see cref="Exception"/> thrown when a <see cref="IChecksumItem"/> failed to verify its checksum.
    /// </summary>
    public class ChecksumViolationException : Exception {

        /// <summary>
        /// Get the checksum hex value causing the exception.
        /// </summary>
        public string Checksum => this.ChecksumValue.ToString("X8", CultureInfo.InvariantCulture);

        /// <summary>
        /// Get the expected checksum hex value.
        /// </summary>
        public string Expected => this.ExpectedValue.ToString("X8", CultureInfo.InvariantCulture);

        /// <summary>
        /// Get the decimal checksum value.
        /// </summary>
        public ulong ChecksumValue { get; }

        /// <summary>
        /// Get the decimal expected checksum value.
        /// </summary>
        public ulong ExpectedValue { get; }

        /// <summary>
        /// Get if this violation is likely caused by lacking data.
        /// </summary>
        public bool MissingData => this.ExpectedValue - this.ChecksumValue > 0;

        /// <summary>
        /// Get if this violation is likely caused by added data.
        /// </summary>
        public bool AddedData => !this.MissingData;

        /// <summary>
        /// New <see cref="ChecksumViolationException"/> instance.
        /// </summary>
        public ChecksumViolationException(ulong calculated, ulong expected) 
            : base($"Checksum verification error! (Expected '{expected:X8}' but got '{calculated:X8}')") {
            this.ExpectedValue = expected;
            this.ChecksumValue = calculated;
        }

    }

}
