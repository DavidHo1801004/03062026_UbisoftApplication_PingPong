using System;
using System.Collections.Generic;

namespace Developer.Collections.Events
{
    /// <summary>
    /// Generic class for handling event subscription and publishing. <br/>
    /// Events are registered in unique channels using a key value pair collection.
    /// </summary>
    /// <typeparam name="TKey"> Typename used to define unique channels. </typeparam>
    /// <typeparam name="TCallbackContext"> Typename used to define delegate: 
    ///                                     <code>void Delegate(TCallbackContext)</code> </typeparam>
    public class EventBus<TKey, TCallbackContext>
    {
        private readonly Dictionary<TKey, Action<TCallbackContext>> handlers = new();

        /// <summary>
        /// Register a new callback handler to an existing or new channel.
        /// </summary>
        /// <param name="_ChannelKey"> Key to define target channel. </param>
        /// <param name="_Handler"> Callback handler to register. </param>
        public void Subscribe(TKey _ChannelKey, Action<TCallbackContext> _Handler)
        {
            if (handlers.TryGetValue(_ChannelKey, out var existing))
                handlers[_ChannelKey] = existing + _Handler;
            else
                handlers[_ChannelKey] = _Handler;
        }

        /// <summary>
        /// Unregister a callback handler from an existing channel.
        /// </summary>
        /// <param name="_ChannelKey"> Key to define target channel. </param>
        /// <param name="_Handler"> Callback handler to unregister. </param>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        public bool Unsubscribe(TKey _ChannelKey, Action<TCallbackContext> _Handler)
        {
            if (!handlers.TryGetValue(_ChannelKey, out var existing)) return false;

            existing -= _Handler;

            if (existing == null)
                handlers.Remove(_ChannelKey);
            else
                handlers[_ChannelKey] = existing;

            return true;
        }

        /// <summary>
        /// Publish event with the given context for the target channel if valid.
        /// </summary>
        public void Publish(TKey _ChannelKey, TCallbackContext _Context)
        {
            if (handlers.TryGetValue(_ChannelKey, out var @event))
                @event?.Invoke(_Context);
        }

        /// <summary>
        /// Check if a given channel key is registered. <br/>
        /// The event channel could still be <see langword="null"/>.
        /// </summary>
        public bool ContainsKey(TKey _ChannelKey)
        {
            return handlers.ContainsKey(_ChannelKey);
        }
    }
}
