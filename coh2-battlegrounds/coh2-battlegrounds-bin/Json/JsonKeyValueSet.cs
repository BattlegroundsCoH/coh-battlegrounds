using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Battlegrounds.Json {

    /// <summary>
    /// 
    /// </summary>
    public class JsonKeyValueSet : IDictionary<IJsonElement, IJsonElement>, IJsonElement {

        private Dictionary<IJsonElement, IJsonElement> m_contents;

        public JsonKeyValueSet() {
            m_contents = new Dictionary<IJsonElement, IJsonElement>();
        }

        public void Populate(object target, Type keyType, Type valueType, Func<string, object> derefMethood) {

            MethodInfo addMethod = target.GetType().GetMethod("Add", new Type[] { keyType, valueType });

            if (addMethod == null) {
                throw new Exception();
            }

            // WARNING!
            // This is temporary code and will cause errors!
            foreach (var pair in m_contents) {

                object[] @params = new object[2];

                @params[0] = Convert.ChangeType(pair.Key, keyType);
                @params[1] = Convert.ChangeType(pair.Value, keyType);

                addMethod.Invoke(target, @params);

            }

        }

        public IJsonElement this[IJsonElement key] { get => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents)[key]; set => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents)[key] = value; }

        public ICollection<IJsonElement> Keys => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).Keys;

        public ICollection<IJsonElement> Values => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).Values;

        public int Count => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).IsReadOnly;

        public void Add(IJsonElement key, IJsonElement value) => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).Add(key, value);
        public void Add(KeyValuePair<IJsonElement, IJsonElement> item) => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).Add(item);
        public void Clear() => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).Clear();
        public bool Contains(KeyValuePair<IJsonElement, IJsonElement> item) => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).Contains(item);
        public bool ContainsKey(IJsonElement key) => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).ContainsKey(key);
        public void CopyTo(KeyValuePair<IJsonElement, IJsonElement>[] array, int arrayIndex) => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<IJsonElement, IJsonElement>> GetEnumerator() => ((IEnumerable<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).GetEnumerator();


        public bool Remove(IJsonElement key) => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).Remove(key);
        public bool Remove(KeyValuePair<IJsonElement, IJsonElement> item) => ((ICollection<KeyValuePair<IJsonElement, IJsonElement>>)this.m_contents).Remove(item);
        public bool TryGetValue(IJsonElement key, out IJsonElement value) => ((IDictionary<IJsonElement, IJsonElement>)this.m_contents).TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_contents).GetEnumerator();

        public override string ToString() => this.m_contents.ToString();

    }

}
