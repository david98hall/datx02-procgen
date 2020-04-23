namespace Services
{
    /// <summary>
    /// An event bus that can notify its subscribers of various events.
    /// </summary>
    /// <typeparam name="T">The event id type.</typeparam>
    public interface IEventBus<T> : IService<T>
    {
        /// <summary>
        /// Creates an event and notifies all subscribers of it.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="eventData">The data of the event.</param>
        void CreateEvent(T eventId, object eventData);

    }
}