using UnityEditor;

namespace Utils.Concurrency
{
    /// <summary>
    /// Continuously invokes the Dispatcher at every Unity update.
    /// </summary>
    [InitializeOnLoad]
    public sealed class DispatchInvoker
    {
        static DispatchInvoker()
        {
            // Add the Dispatcher InvokePending method to the editor's update function
            EditorApplication.update += Dispatcher.Instance.InvokePending;
        }
    }
}