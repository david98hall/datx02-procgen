using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Concurrency
{
    /// <summary>
    /// Utility methods for tasks.
    /// </summary>
    public static class TaskUtils
    {
        /// <summary>
        /// Runs a task for each input item, executing the output action.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="outputAction">The action to apply on each input item.</param>
        /// <typeparam name="TI">The input type.</typeparam>
        /// <typeparam name="TO">The output type.</typeparam>
        /// <returns>Each task's output.</returns>
        public static IEnumerable<TO> RunActionInTasks<TI, TO>(IEnumerable<TI> input, Func<TI, TO> outputAction)
        {
            return RunActionInTasks(input, outputAction, CancellationToken.None);
        }

        /// <summary>
        /// Runs a task for each input item, executing the output action.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="outputAction">The action to apply on each input item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <typeparam name="TI">The input type.</typeparam>
        /// <typeparam name="TO">The output type.</typeparam>
        /// <returns>Each task's output.</returns>
        public static IEnumerable<TO> RunActionInTasks<TI, TO>(
            IEnumerable<TI> input, 
            Func<TI, TO> outputAction,
            CancellationToken cancellationToken)
        {
            // Run one task per input item
            var tasks = new LinkedList<Task>();
            foreach (var item in input)
            {
                // Cancel if requested
                if (cancellationToken.IsCancellationRequested) return null;
                
                tasks.AddLast(Task.Run(() => outputAction(item), cancellationToken));
            }
            
            // Wait for all tasks to finish
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
            
            // Extract the result from each task and return it and cancel if requested
            return cancellationToken.IsCancellationRequested 
                ? null 
                : tasks.Select(task => ((Task<TO>) task).Result);
        }
        
    }
}