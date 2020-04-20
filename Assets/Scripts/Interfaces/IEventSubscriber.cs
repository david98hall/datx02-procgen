namespace Interfaces
{
    public interface IEventSubscriber<in T>
    {
        void OnNotification(T eventId, object eventData);
    }
}