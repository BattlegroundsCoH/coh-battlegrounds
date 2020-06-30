using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Battlegrounds.Game.Database.json {
    
    /// <summary>
    /// Represents an array for <see cref="IJsonElement"/> elements. Implements <see cref="IList"/> and <see cref="IJsonElement"/>.
    /// </summary>
    public class JsonArray : IList<IJsonElement>, IJsonElement {

        private List<IJsonElement> m_contents;

        /// <summary>
        /// The first <see cref="IJsonElement"/> in the array.
        /// </summary>
        public IJsonElement First => m_contents[0];

        /// <summary>
        /// Create new empty <see cref="JsonArray"/> instance.
        /// </summary>
        public JsonArray() {
            m_contents = new List<IJsonElement>();
        }

        /// <summary>
        /// Populate an enumerable with all the contents from the <see cref="JsonArray"/>.
        /// </summary>
        /// <param name="enumerable">The enumerable to populate with array elements.</param>
        /// <param name="objType">The object type expected by the enumerable.</param>
        /// <param name="refMethod">The json dereference method when adding the element.</param>
        /// <remarks>The 'enumerable' object must have an 'Add' method with one argument. The result value of the 'Add' method is ignored.</remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public void Populate(object enumerable, Type objType, Func<string, object> refMethod = null) {

            // Make sure the enumerable exists
            if (enumerable == null)
                throw new ArgumentNullException("Argument 'enumerable' was null.");

            if (objType == null)
                throw new ArgumentNullException("Argument 'objType' was null.");

            // Find the add method
            MethodInfo addMethod = enumerable.GetType().GetMethod("Add");

            // If no add method was found, throw a not supported exception
            if (addMethod == null)
                throw new ArgumentException($"{enumerable.GetType().Name} does not have an 'Add' method.");

            // Loop through all the elements
            foreach (IJsonElement elem in m_contents) {

                // Is it a json value
                if (elem is JsonValue jval) {

                    // Use the reference method
                    if (refMethod != null) {

                        // Invoke add method to add element and invoke the dereference method
                        addMethod.Invoke(enumerable, new object[] { refMethod.Invoke(jval.StringValue) });

                    } else {

                        // Invoke add method where value is changed
                        addMethod.Invoke(enumerable, new object[] { Convert.ChangeType(elem, objType) });

                    }

                } else {

                    // Add the element
                    addMethod.Invoke(enumerable, new object[] { elem  });

                }

            }

        }

        /// <summary>
        /// Get or set the <see cref="IJsonElement"/> found at the specified index position in the array.
        /// </summary>
        /// <param name="index">The index of the element to get or set value of.</param>
        /// <returns>The <see cref="IJsonElement"/> found at specified array index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public IJsonElement this[int index] { get => ((IList<IJsonElement>)this.m_contents)[index]; set => ((IList<IJsonElement>)this.m_contents)[index] = value; }

        /// <summary>
        /// Get the current amount of items in the array.
        /// </summary>
        public int Count => this.m_contents.Count;

        /// <summary>
        /// Is this array considered to be readonly.
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// Adds the <see cref="IJsonElement"/> to the first available position in the array.
        /// </summary>
        /// <param name="item">The new <see cref="IJsonElement"/> to add to the array.</param>
        public void Add(IJsonElement item) => ((ICollection<IJsonElement>)this.m_contents).Add(item);

        /// <summary>
        /// Clears the array of all elements.
        /// </summary>
        public void Clear() => ((ICollection<IJsonElement>)this.m_contents).Clear();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(IJsonElement item) => ((ICollection<IJsonElement>)this.m_contents).Contains(item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(IJsonElement[] array, int arrayIndex) => ((ICollection<IJsonElement>)this.m_contents).CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="JsonArray"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator<IJsonElement> GetEnumerator() => ((IEnumerable<IJsonElement>)this.m_contents).GetEnumerator();

        /// <summary>
        /// Finds the index position of the <see cref="IJsonElement"/> element.
        /// </summary>
        /// <param name="item">The <see cref="IJsonElement"/> to find the index of.</param>
        /// <returns>The index position if it was found. Otherwise -1 if the position was not found.</returns>
        public int IndexOf(IJsonElement item) => ((IList<IJsonElement>)this.m_contents).IndexOf(item);

        /// <summary>
        /// Insert a <see cref="IJsonElement"/> at specified index.
        /// </summary>
        /// <param name="index">The non-negative index to insert item at.</param>
        /// <param name="item">The <see cref="IJsonElement"/> to insert.</param>
        public void Insert(int index, IJsonElement item) => ((IList<IJsonElement>)this.m_contents).Insert(index, item);

        /// <summary>
        /// Remove the <see cref="IJsonElement"/> from the array.
        /// </summary>
        /// <param name="item">The item to remove from the array.</param>
        /// <returns>true if the item was successfully removed.</returns>
        public bool Remove(IJsonElement item) => ((ICollection<IJsonElement>)this.m_contents).Remove(item);

        /// <summary>
        /// Remove the array element at the specified position.
        /// </summary>
        /// <param name="index">The index of the <see cref="IJsonElement"/> to remove.</param>
        public void RemoveAt(int index) => ((IList<IJsonElement>)this.m_contents).RemoveAt(index);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="JsonArray"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_contents).GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.m_contents.Count.ToString();

    }

}
