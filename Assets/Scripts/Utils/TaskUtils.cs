using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Utils
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
            // Run one task per input item
            var tasks = new LinkedList<Task>();
            foreach (var item in input)
            {
                tasks.AddLast(Task.Run(() => outputAction(item)));
            }
            
            // Wait for all tasks to finish
            Task.WaitAll(tasks.ToArray());
            
            // Extract the result from each task and return it
            return tasks.Select(task => ((Task<TO>) task).Result);
        }
    }
}