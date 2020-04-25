using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Concurrency
{
    public sealed class Dispatcher
    {

        public static Dispatcher Instance => instance ?? (instance = new Dispatcher());
        private static Dispatcher instance;

        private readonly Queue<Action> pendingActions;
        
        private readonly Queue<Func<object>> pendingFunctions;
        private readonly ConcurrentDictionary<Func<object>, object> results;
        
        private Dispatcher()
        {
            pendingActions = new Queue<Action>();
            pendingFunctions = new Queue<Func<object>>();
            results = new ConcurrentDictionary<Func<object>, object>();
        }

        public void EnqueueAction(Action action)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(action);
            }
        }
        
        public TO EnqueueFunction<TO>(Func<TO> func)
        {
            object ObjectFunc() => func();
            
            lock (pendingFunctions)
            {
                pendingFunctions.Enqueue(ObjectFunc);
            }

            while (true)
            {
                if (results.TryRemove(ObjectFunc, out var result))
                {
                    return (TO) result;
                }
            }
        }

        public void InvokePending()
        {
            lock (pendingActions)
            {
                foreach (var action in pendingActions)
                {
                    action();
                }
                pendingActions.Clear();
            }
            
            lock (pendingFunctions)
            {
                foreach (var func in pendingFunctions)
                {
                    var result = func();
                    while (results.TryAdd(func, result))
                    {}
                }
                pendingFunctions.Clear();
            }
        }
        
    }
}