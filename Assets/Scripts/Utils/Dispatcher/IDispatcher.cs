using System;

namespace Utils.Dispatching
{
    public interface IDispatcher
    {
        void Invoke(Action action);
    }
}