using System.Collections;
using System.Collections.Generic;

namespace Battlegrounds.Game.Database.json {
    
    /// <summary>
    /// 
    /// </summary>
    public class JsonArray : IList<IJsonElement>, IJsonElement {

        private List<IJsonElement> m_contents;

        /// <summary>
        /// The first <see cref="IJsonElement"/> in the array
        /// </summary>
        public IJsonElement First => m_contents[0];

        /// <summary>
        /// 
        /// </summary>
        public JsonArray() {
            m_contents = new List<IJsonElement>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IJsonElement this[int index] { get => ((IList<IJsonElement>)this.m_contents)[index]; set => ((IList<IJsonElement>)this.m_contents)[index] = value; }

        /// <summary>
        /// 
        /// </summary>
        public int Count => ((ICollection<IJsonElement>)this.m_contents).Count;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(IJsonElement item) => ((ICollection<IJsonElement>)this.m_contents).Add(item);

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IJsonElement> GetEnumerator() => ((IEnumerable<IJsonElement>)this.m_contents).GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(IJsonElement item) => ((IList<IJsonElement>)this.m_contents).IndexOf(item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, IJsonElement item) => ((IList<IJsonElement>)this.m_contents).Insert(index, item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(IJsonElement item) => ((ICollection<IJsonElement>)this.m_contents).Remove(item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index) => ((IList<IJsonElement>)this.m_contents).RemoveAt(index);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_contents).GetEnumerator();

    }

}
