using System;
using System.Threading.Tasks;
using RemoteApi.Monitor;

namespace RemoteApi.Integration2.Helpers
{
    public class RemoteTraceMonitorСonsistentTester : IRemoteTraceMonitorСonsistent
    {
        public int SetPromptCalledTimes { get; private set; }
        
        private TaskCompletionSource<bool> _initializationTask;
        private TaskCompletionSource<bool> _displayMessageTask;

        public event Func<string, Task<bool>> CommandReceived;
        public Task Initialized => _initializationTask.Task;
        public Task MessageDisplayed => _displayMessageTask.Task;
        
        public RemoteTraceMonitorСonsistentTester()
        {
            _initializationTask = new TaskCompletionSource<bool>();
            _displayMessageTask = new TaskCompletionSource<bool>();
        }

        public void Start() { }

        public void Stop() { }

        public async void DisplayNextMessage(string message)
        {
            if (_displayMessageTask == default)
            {
                throw new Exception("display message task null");
            }

            await Task.Delay(100);
            _displayMessageTask.SetResult(true);
        }

        public void DisplayDebugMessage(string message)
        {
            var a = message;
        }

        public void ClearTextBox() { }

        public void SetPrompt(string value)
        {
            _initializationTask.SetResult(true);
            _initializationTask = new TaskCompletionSource<bool>();
            SetPromptCalledTimes++;
        }

        public Task<bool> RaiseCommandReceived(string value)
        {
            _displayMessageTask = new TaskCompletionSource<bool>();
            return CommandReceived?.Invoke(value);;
        }
    }
}