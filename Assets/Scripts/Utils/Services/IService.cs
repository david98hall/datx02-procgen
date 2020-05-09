namespace Services
{
    /// <summary>
    /// A service that can be subscribed to.
    /// </summary>
    /// <typeparam name="T">The input type of the subscribers.</typeparam>
    public interface IService<out T>
    {
        /// <summary>
        /// Subscribe an object to this service.
        /// </summary>
        /// <param name="newSubscriber">The object to subscribe to this service.</param>
        void Subscribe(ISubscriber<T> newSubscriber);
        
        /// <summary>
        /// Unsubscribe an object from this service.
        /// </summary>
        /// <param name="oldSubscriber">The object to unsubscribe from this service.</param>
        void Unsubscribe(ISubscriber<T> oldSubscriber);
    }
}