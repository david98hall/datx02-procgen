using System.Collections.Generic;

namespace BIAS.Utils.Services
{
    /// <summary>
    /// An event bus that can notify its subscribers of various events.
    /// </summary>
    /// <typeparam name="T">The event id type.</typeparam>
    public class EventBus<T> : IEventBus<T>
    {
        private readonly ICollection<ISubscriber<T>> _subscribers = new HashSet<ISubscriber<T>>();
        
        public void CreateEvent(T eventId, object eventData, object creator)
        {
            foreach (var eventSubscriber in _subscribers)
            {
                eventSubscriber.OnEvent(eventId, eventData, creator);
            }
        }

        public void Subscribe(ISubscriber<T> newSubscriber)
        {
            _subscribers.Add(newSubscriber);
        }
        
        public void Unsubscribe(ISubscriber<T> oldSubscriber)
        {
            _subscribers.Remove(oldSubscriber);
        }
    }
}