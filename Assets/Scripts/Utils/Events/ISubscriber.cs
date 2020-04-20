namespace Utils.Events
{
    /// <summary>
    /// Listens to events created by its subscriptions.
    /// </summary>
    /// <typeparam name="T">The event id type.</typeparam>
    public interface ISubscriber<in T>
    {
        /// <summary>
        /// Reacts to an event created by a subscription.
        /// </summary>
        /// <param name="eventId">The id of the event.</param>
        /// <param name="eventData">The data of the event.</param>
        void OnEvent(T eventId, object eventData);
    }
}