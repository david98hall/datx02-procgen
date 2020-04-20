namespace Interfaces
{
    /// <summary>
    /// A service that can be subscribed to.
    /// </summary>
    /// <typeparam name="T">The input type of the subscribers.</typeparam>
    public interface IService<out T>
    {
        void Subscribe(IEventSubscriber<T> eventSubscriber);
        
        void Unsubscribe(IEventSubscriber<T> eventSubscriber);
    }
}