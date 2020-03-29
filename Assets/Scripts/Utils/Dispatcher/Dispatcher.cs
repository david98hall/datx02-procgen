using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Dispatching
{
    public class Dispatcher : IDispatcher
    {

        private static Dispatcher instance;

        public static Dispatcher Instance => instance ?? (instance = new Dispatcher());

        private readonly Queue<Action> pendingActions;

        private Dispatcher()
        {
            pendingActions = new Queue<Action>();
        }

        public void Invoke(Action action)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(action);
            }
        }

        public void InvokePending()
        {
            lock (pendingActions)
            {
                while (pendingActions.Any())
                {
                    pendingActions.Dequeue().Invoke();
                }
            }
        }
    }
}