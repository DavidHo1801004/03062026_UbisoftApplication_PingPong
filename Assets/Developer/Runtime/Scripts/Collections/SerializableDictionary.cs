using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Developer.Collections
{
    /// <summary>
    /// Serializable dictionary collection. <br/>
    /// Used for Unity serialization system only, does not support editor controls.
    /// </summary>
    /// <typeparam name="TKey"> Key type. </typeparam>
    /// <typeparam name="TValue"> Value type. </typeparam>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector]
        private List<TKey> keys = new();

        [SerializeField, HideInInspector] 
        private List<TValue> values = new();

        private readonly Dictionary<TKey, TValue> dictionary = new();

        public Dictionary<TKey, TValue>.KeyCollection Keys
            => dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values
            => dictionary.Values;

        public TValue this[TKey _Key]
        {
            get => dictionary[_Key];
            set => dictionary[_Key] = value;
        }

        public int Count => dictionary.Count;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
            => ((IDictionary<TKey, TValue>)dictionary).Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values
            => ((IDictionary<TKey, TValue>)dictionary).Values;

        public bool IsReadOnly
            => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;



        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serializableDictionary)
            => serializableDictionary.dictionary;



        #region ISerializationCallbackReceiver
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary[keys[i]] = values[i];
            }
        }
        #endregion

        #region IDictionary
        public void Add(TKey _Key, TValue _Value)
        {
            ((IDictionary<TKey, TValue>)dictionary).Add(_Key, _Value);
        }

        public bool ContainsKey(TKey _Key)
        {
            return ((IDictionary<TKey, TValue>)dictionary).ContainsKey(_Key);
        }

        public bool Remove(TKey _Key)
        {
            return ((IDictionary<TKey, TValue>)dictionary).Remove(_Key);
        }

        public bool TryGetValue(TKey _Key, out TValue _Value)
        {
            return ((IDictionary<TKey, TValue>)dictionary).TryGetValue(_Key, out _Value);
        }

        public void Add(KeyValuePair<TKey, TValue> _Item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(_Item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> _Item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(_Item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] _Array, int _ArrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(_Array, _ArrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> _Item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(_Item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dictionary).GetEnumerator();
        }
        #endregion
    }
}