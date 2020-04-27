using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Utils.Concurrency
{
    /// <summary>
    /// A thread-safe singleton type used to dispatch actions
    /// created on other threads to the main thread of Unity.
    /// </summary>
    public sealed class Dispatcher
    {
        /// <summary>
        /// The only instance of this type.
        /// </summary>
        public static Dispatcher Instance => instance ?? (instance = new Dispatcher());
        private static Dispatcher instance;

        // Pending actions and functions
        private readonly Queue<Action> pendingActions;
        private readonly Queue<Func<object>> pendingFunctions;
        
        // Finished actions and functions
        private readonly ConcurrentDictionary<Func<object>, object> results;
        private readonly ConcurrentDictionary<Action, byte> finishedActions;
        
        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private Dispatcher()
        {
            pendingActions = new Queue<Action>();
            pendingFunctions = new Queue<Func<object>>();
            results = new ConcurrentDictionary<Func<object>, object>();
            finishedActions = new ConcurrentDictionary<Action, byte>();
        }

        /// <summary>
        /// Enqueues an action to be dispatched to the main thread of Unity.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        public void EnqueueAction(Action action)
        {
            if (action == null) return;
            
            lock (pendingActions)
            {
                // Thread-safe enqueue
                pendingActions.Enqueue(action);
            }
        }

        /// <summary>
        /// Enqueues an action to be dispatched to the main thread of Unity.
        /// Waits until the action has been invoked by the main thread.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        public void EnqueueActionAndWait(Action action)
        {
            if (action == null) return;
            
            EnqueueAction(action);

            // Busy-wait until the action has been removed from the
            // set of finished actions. It is only added to it if it has finished.
            while (true)
            {
                if (finishedActions.TryRemove(action, out _))
                    break;
            }
        }
        
        /// <summary>
        /// Enqueues a function to be dispatched to the main thread of Unity.
        /// Waits until the function has been called and returns its result.
        /// </summary>
        /// <param name="func">The function to invoke on the main thread.</param>
        /// <typeparam name="TO">The return type of the function.</typeparam>
        /// <returns>The result of the invoked function.</returns>
        public TO EnqueueFunction<TO>(Func<TO> func)
        {
            if (func == null)
                throw new NullReferenceException("func cannot be null!");
            
            // The same function, but returning an object
            object ObjectFunc() => func();
            
            lock (pendingFunctions)
            {
                // Thread-safe enqueue of the function
                pendingFunctions.Enqueue(ObjectFunc);
            }

            // Busy-wait until the function has been invoked and finished
            while (true)
            {
                if (results.TryRemove(ObjectFunc, out var result))
                {
                    // Return the function's result
                    return (TO) result;
                }
            }
        }

        /// <summary>
        /// Invokes all pending actions and functions.
        /// Meant to be called on Unity's main thread.
        /// </summary>
        public void InvokePending()
        {
            lock (pendingActions)
            {
                // Invoke all pending actions in a thread-safe manner
                foreach (var action in pendingActions)
                {
                    action();
                    
                    // Busy-wait until the action is marked as "finished"
                    while (!finishedActions.TryAdd(action, 0))
                    {}
                }
                pendingActions.Clear();
            }
            
            lock (pendingFunctions)
            {
                // Invoke all pending functions in a thread-safe manner
                foreach (var func in pendingFunctions)
                {
                    // Get the function's result
                    var result = func();
                    
                    // Busy-wait until the function is marked as "finished"
                    while (!results.TryAdd(func, result))
                    {}
                }
                pendingFunctions.Clear();
            }
        }
        
    }
}