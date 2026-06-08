using System;
using System.Collections.Generic;

namespace Developer.Collections
{
    /// <summary>
    /// Generic shared data storage that ensure compile-time type safety.
    /// </summary>
    [System.Serializable]
    public sealed class Blackboard
    {
        /// <summary>
        /// Blackboard key. Used for object reference only. <br/>
        /// Use <see cref="Key{T}"/> to create valid blackboard key.
        /// </summary>
        public abstract class Key
        {
            public string Name { get; }

            protected abstract internal string ID { get; }

            // Intentionally hidden. Should only be used for callback purposes
            protected Key(string _Name)
            {
                Name = _Name;
            }
        }

        /// <summary>
        /// Typed blackboard key. Used to create unique blackboard keys.
        /// </summary>
        /// <typeparam name="T"> Target type of the blackboard key </typeparam>
        public sealed class Key<T> : Key
        {
            public Key(string _Name) : base(_Name) { }

            protected internal override string ID => $"{Name} ({typeof(T).Name})";

            public override string ToString() => ID;
        }

        /// <summary>
        /// Contains callback data after a blackboard property modification.
        /// </summary>
        public readonly struct PropertyChangedCallback<T>
        {
            public readonly Key<T> key;
            public readonly object oldValue;
            public readonly object newValue;

            public PropertyChangedCallback(
                Key<T> _Key,
                object _OldValue,
                object _NewValue)
            {
                key = _Key; 
                oldValue = _OldValue; 
                newValue = _NewValue;
            }
        }



        [UnityEngine.SerializeField]
        private SerializableDictionary<string, object> data = new();

        private readonly Dictionary<Key, Delegate> valueChangedCallbackListeners = new();

        /// <summary>
        /// Called when a new key is added. <br/>
        /// For value change callback, use <see cref="RegisterValueChangeCallback{T}(Key{T}, Action{PropertyChangedCallback{T}})"/> instead.
        /// </summary>
        public event Action<Key> OnKeyAdded;

        /// <summary>
        /// Called when an existing key is removed. <br/>
        /// For value change callback, use <see cref="RegisterValueChangeCallback{T}(Key{T}, Action{PropertyChangedCallback{T}})"/> instead.
        /// </summary>
        public event Action<Key> OnKeyRemoved;



        /// <summary>
        /// Add an empty value key to the blackboard.
        /// </summary>
        /// <param name="_Key"> The key to add. </param>
        public bool Add<T>(Key<T> _Key)
        {
            if (data.ContainsKey(_Key.ID)) return false;

            data.Add(_Key.ID, null);
            OnKeyAdded?.Invoke(_Key);

            return true;
        }

        /// <summary>
        /// Sets a value in the blackboard if the key exists, or adds a new key-value pair if it does not. 
        /// </summary>
        /// <param name="_Key"> The key associated with the value. </param>
        /// <param name="_Value"> The value to set or add. </param>
        public void SetOrAdd<T>(Key<T> _Key, T _Value)
        {
            if (!HasKey(_Key))
            {
                data.Add(_Key.ID, null);
                OnKeyAdded?.Invoke(_Key);
            }

            Set(_Key, _Value);
        }

        /// <summary>
        /// Sets a value in the blackboard if the key exists. 
        /// </summary>
        /// <param name="_Key"> The key associated with the value. </param>
        /// <param name="_Value"> The value to set. </param>
        /// <returns> 
        /// <see langword="true"/> if the key exists and the value was set; <br/>
        /// otherwise, <see langword="false"/>. 
        /// </returns>
        public bool Set<T>(Key<T> _Key, T _Value)
        {
            if (!HasKey(_Key)) return false;

            T oldValue = Get(_Key);
            data[_Key.ID] = _Value;

            if (valueChangedCallbackListeners.TryGetValue(_Key, out var del))
            {
                ((Action<PropertyChangedCallback<T>>)del)?.Invoke(
                    new PropertyChangedCallback<T>(
                        _Key,
                        oldValue,
                        _Value));
            }

            return true;
        }

        /// <summary>
        /// Retrieves a value from the blackboard. 
        /// </summary>
        /// <param name="_Key"> The key associated with the value. </param>
        /// <param name="_DefaultValue"> The default value if the key is not found. </param>
        /// <returns> 
        /// The retrieved value if found; <br/>
        /// otherwise, the provided default value. 
        /// </returns>
        public T Get<T>(Key<T> _Key, T _DefaultValue = default)
        {
            if (data.TryGetValue(_Key.ID, out object value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Attempts to retrieve a value associated with the given key.
        /// </summary>
        /// <param name="_Key"> The key to look up in the data storage. </param>
        /// <param name="_Value"> The retrieved value if the key exists and is of the correct type. </param>
        /// <returns>
        /// <see langword="true"/> if the key exists and the value is successfully retrieved; <br/>
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetValue<T>(Key<T> _Key, out T _Value)
        {
            _Value = default;

            if (!data.TryGetValue(_Key.ID, out object value)) return false;
            if (value is not T typedValue) return false;

            _Value = typedValue;
            return true;
        }

        /// <summary>
        /// Checks whether the blackboard contains the specified key. 
        /// </summary>
        /// <param name="_Key"> The blackboard key to check. </param>
        /// <returns> 
        /// <see langword="true"/> if the key exists; <br/>
        /// otherwise, <see langword="false"/>. 
        /// </returns>
        public bool HasKey(Key _Key) => data.ContainsKey(_Key.ID);

        /// <summary>
        /// Check whether the given key exist and valid within the blackboard. <br/>
        /// <b>NOTE:</b> A key is considered valid if the data attached to it is not <see langword="null"/>.
        /// </summary>
        /// <param name="_Key"> The key to check. </param>
        /// <returns>
        /// <see langword="true"/> if the key exists and is valid; <br/>
        /// otherwise, <see langword="false"/>. 
        /// </returns>
        public bool HasValidKey(Key _Key) => HasKey(_Key) && data[_Key.ID] != null;

        /// <summary>
        /// Removes a key and its associated value from the blackboard. 
        /// </summary>
        /// <param name="_Key"> The key to remove. </param>
        public bool Remove<T>(Key<T> _Key)
        {
            if (!data.ContainsKey(_Key.ID)) return false;

            data.Remove(_Key.ID);
            OnKeyRemoved?.Invoke(_Key);

            return true;
        }

        /// <summary>
        /// Clear the value of a given key. <br/>
        /// This does not remove the key from the blackboard, so using <see cref="HasKey(Key)"/> would still returns <see langword="true"/>. <br/>
        /// To remove the key, use <see cref="Remove(Key)"/> instead.
        /// </summary>
        /// <param name="_Key"> The key to invalidate. </param>
        public void Invalidate<T>(Key<T> _Key)
        {
            if (!data.ContainsKey(_Key.ID)) return;

            T oldValue = Get(_Key);
            data[_Key.ID] = null;

            if (valueChangedCallbackListeners.TryGetValue(_Key, out var del))
            {
                ((Action<PropertyChangedCallback<T>>)del)?.Invoke(
                    new PropertyChangedCallback<T>(
                        _Key,
                        oldValue,
                        null));
            }
        }

        /// <summary>
        /// Clear all data in the blackboard.
        /// </summary>
        public void Clear()
        {

            data.Clear();
        }

        /// <summary>
        /// Add a value change callback to a property key if exist.
        /// </summary>
        /// <typeparam name="T"> Property value type. </typeparam>
        /// <param name="_Key"> The property key to retrieve. </param>
        /// <param name="_Callback"> The assigned callback. </param>
        public void RegisterValueChangeCallback<T>(
            Key<T> _Key,
            Action<PropertyChangedCallback<T>> _Callback)
        {
            if (valueChangedCallbackListeners.TryGetValue(_Key, out var existing))
                valueChangedCallbackListeners[_Key] = Delegate.Combine(existing, _Callback);
            else
                valueChangedCallbackListeners[_Key] = _Callback;
        }

        /// <summary>
        /// Remove a value change callback from a property key if exist.
        /// </summary>
        /// <typeparam name="T"> Property value type. </typeparam>
        /// <param name="_Key"> The property key to retrieve. </param>
        /// <param name="_Callback"> The assigned callback. </param>
        public void UnregisterValueChangeCallback<T>(
            Key<T> _Key,
            Action<PropertyChangedCallback<T>> _Callback)
        {
            if (!valueChangedCallbackListeners.TryGetValue(_Key, out var existing)) return;

            valueChangedCallbackListeners[_Key] = Delegate.Remove(existing, _Callback);
        }



        // DEBUG
        internal void PrintAll()
        {
            foreach (var pair in data)
            {
                string suffix;
                if (pair.Value == null)
                    suffix = "Not Set";
                else
                    suffix = "Set";
                UnityEngine.Debug.Log($"{pair.Key}: {suffix}");
            }
        }
    }
}
