using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using Battlegrounds.Functional;

namespace Battlegrounds.Verification {

    /// <summary>
    /// Class that can calculate the numeric checksum value of an object implementing <see cref="IChecksumItem"/>.
    /// </summary>
    public class Checksum {

        /// <summary>
        /// Readonly struct representing a property in a <see cref="IChecksumItem"/>.
        /// </summary>
        public readonly struct Property {

            /// <summary>
            /// Get the value of the checksum property.
            /// </summary>
            public object Value { get; }

            /// <summary>
            /// Get the attribute object associated with the property.
            /// </summary>
            public ChecksumPropertyAttribute ChecksumAttribute { get; }

            /// <summary>
            /// Initialise a new <see cref="Property"/> instance with <paramref name="val"/> and <paramref name="attrib"/> specified.
            /// </summary>
            /// <param name="val">The actual value of the property.</param>
            /// <param name="attrib">The attribute associated with the property.</param>
            public Property(object val, ChecksumPropertyAttribute attrib) {
                this.Value = val;
                this.ChecksumAttribute = attrib;
            }

        }

        private readonly List<object> m_sumItems;

        /// <summary>
        /// Initialise a new and empty <see cref="Checksum"/> class.
        /// </summary>
        public Checksum() => this.m_sumItems = new();

        /// <summary>
        /// Initialise a new <see cref="Checksum"/> class based on the <see cref="IChecksumItem"/> <paramref name="item"/> instance.
        /// </summary>
        /// <param name="item">The <see cref="IChecksumItem"/> to represent.</param>
        public Checksum(IChecksumItem item) : this() {
            GetChecksumProperties(item).ForEach(x => this.AddValue(x));
        }

        /// <summary>
        /// Add a new <see cref="Property"/> to the checksum calculation.
        /// </summary>
        /// <param name="property">The property to consider when calculating the checksum.</param>
        public void AddValue(Property property) {
            if (property.ChecksumAttribute.IsCollection) {
                var enumerator = (property.Value as IEnumerable).GetEnumerator();
                int count = 0;
                while (enumerator.MoveNext()) {
                    if (enumerator.Current is IChecksumPropertyItem item) {
                        GetChecksumProperties(item).ForEach(x => this.AddValue(x));
                    } else {
                        this.m_sumItems.Add(enumerator.Current);
                    }
                    count++;
                }
                this.m_sumItems.Add(count);
            } else {
                this.m_sumItems.Add(property.Value);
            }
        }

        /// <summary>
        /// Calculate the actual checksum based on the added values.
        /// </summary>
        /// <returns>The numeric checksum value represented by the added values.</returns>
        public ulong GetCheckksum() {
            StringBuilder str = new();
            for (int i = 0; i < this.m_sumItems.Count; i++) {
                str.Append(this.m_sumItems[i] switch {
                    float f => f.ToString(CultureInfo.InvariantCulture),
                    double d => d.ToString(CultureInfo.InvariantCulture),
                    TimeSpan t => t.ToString("c", CultureInfo.InvariantCulture),
                    null => string.Empty,
                    _ => this.m_sumItems[i].ToString(),
                });
            }
            return Encoding.UTF8.GetBytes(str.ToString()).Aggregate(0ul, (a,b) => a + b);
        }

        /// <summary>
        /// Collect all <see cref="ChecksumPropertyAttribute"/> valid properties from <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to collect property data from.</param>
        /// <returns>An array of property data that can be used for getting checksum values.</returns>
        public static Property[] GetChecksumProperties(IChecksumPropertyItem item) {
            var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<ChecksumPropertyAttribute>() is not null).ToArray();
            Property[] arr = new Property[properties.Length];
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = new(properties[i].GetValue(item), properties[i].GetCustomAttribute<ChecksumPropertyAttribute>());
            }
            return arr;
        }

    }

}
