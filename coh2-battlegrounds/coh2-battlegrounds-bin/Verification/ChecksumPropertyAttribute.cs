using System;

namespace Battlegrounds.Verification;

/// <summary>
/// Attribute marking a property to be considered when calculating the checksum.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChecksumPropertyAttribute : Attribute {

    /// <summary>
    /// Get or set if the checksum property value is a collection (Can be iterated over).
    /// </summary>
    public bool IsCollection { get; set; } = false;

}

