using System;
using System.Collections.Generic;

namespace Developer.Collections.Events
{
    /// <summary>
    /// Events are identified by their context type and exposed through
    /// a single aggregation point.
    /// </summary>
    public class EventAggregator
    {
        private readonly Dictionary<Type, Delegate> handlers = new();

        /// <summary>
        /// Register a new callback handler for an event type.
        /// </summary>
        /// <typeparam name="TEvent"> Typename used to define the event context. </typeparam>
        /// <param name="_Handler"> Callback handler to register. </param>
        public void Subscribe<TEvent>(Action<TEvent> _Handler)
        {
            var eventType = typeof(TEvent);

            if (handlers.TryGetValue(eventType, out var existing))
                handlers[eventType] = Delegate.Combine(existing, _Handler);
            else
                handlers[eventType] = _Handler;
        }

        /// <summary>
        /// Unregister a callback handler from an event type.
        /// </summary>
        /// <typeparam name="TEvent"> Typename used to define the event context. </typeparam>
        /// <param name="_Handler"> Callback handler to unregister. </param>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        public bool Unsubscribe<TEvent>(Action<TEvent> _Handler)
        {
            var eventType = typeof(TEvent);

            if (!handlers.TryGetValue(eventType, out var existing))
                return false;

            var updated = Delegate.Remove(existing, _Handler);

            if (updated == null)
                handlers.Remove(eventType);
            else
                handlers[eventType] = updated;

            return true;
        }

        /// <summary>
        /// Publish an event to all registered subscribers.
        /// </summary>
        /// <typeparam name="TEvent"> Typename used to define the event context. </typeparam>
        /// <param name="_Event"> Event context to publish. </param>
        public void Publish<TEvent>(TEvent _Event)
        {
            if (handlers.TryGetValue(typeof(TEvent), out var @event))
                ((Action<TEvent>)@event)?.Invoke(_Event);
        }

        /// <summary>
        /// Check if a given event type is registered.
        /// </summary>
        public bool Contains<TEvent>()
        {
            return handlers.ContainsKey(typeof(TEvent));
        }
    }
}
