namespace Interfaces
{
    public interface IEventBus<T> : IService<T>
    {
        void Notify(T eventId, object eventData);
    }
}