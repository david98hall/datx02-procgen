using UnityEditor;

namespace Utils.Concurrency
{
    [InitializeOnLoad]
    public sealed class DispatchUpdater
    {
        static DispatchUpdater()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            Dispatcher.Instance.InvokePending();
        }
    }
}