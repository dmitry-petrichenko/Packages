using System;
using System.Threading.Tasks;
using RemoteApi.Monitor;

namespace RemoteApi.Integration.Helpers
{
    public class RemoteTraceMonitorСonsistentTester : IRemoteTraceMonitorСonsistent
    {
        public int SetPromptCalledTimes { get; private set; }
        
        private TaskCompletionSource<bool> _initializationTask;
        
        public RemoteTraceMonitorСonsistentTester()
        {
            _initializationTask = new TaskCompletionSource<bool>();
        }

        public void Start() { }

        public void Stop() { }

        public void DisplayNextMessage(string message) { }

        public void DisplayDebugMessage(string message) { }

        public void ClearTextBox() { }

        public void SetPrompt(string value)
        {
            _initializationTask.SetResult(true);
            _initializationTask = new TaskCompletionSource<bool>();
            SetPromptCalledTimes++;
        }

        public event Func<string, Task<bool>> CommandReceived;
        public Task Initialized => _initializationTask.Task;

        public Task<bool> RaiseCommandReceived(string value)
        {
            return CommandReceived?.Invoke(value);
        }
    }
}