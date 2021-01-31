using System;
using System.Threading.Tasks;
using RemoteApi.Monitor;

namespace RemoteApi.Integration.Helpers
{
    public class RemoteTraceMonitorСonsistentTester : IRemoteTraceMonitorСonsistent
    {
        public int SetPromptCalledTimes { get; private set; }
        
        private TaskCompletionSource<bool> _initializationTask;
        private TaskCompletionSource<bool> _displayMessageTask;

        private readonly ApplicationCacheRecorder _applicationCacheRecorder;

        public event Func<string, Task<bool>> CommandReceived;
        public Task Initialized => _initializationTask.Task;
        public Task MessageDisplayed => _displayMessageTask.Task;
        
        public RemoteTraceMonitorСonsistentTester(ApplicationCacheRecorder applicationCacheRecorder)
        {
            _applicationCacheRecorder = applicationCacheRecorder;
            _initializationTask = new TaskCompletionSource<bool>();
            _displayMessageTask = new TaskCompletionSource<bool>();
        }

        public void Start() { }

        public void Stop() { }

        public async void DisplayNextMessage(string message)
        {
            _applicationCacheRecorder.DisplayNextMessage(message);
            if (_displayMessageTask == default)
            {
                throw new Exception("display message task null");
            }

            await Task.Delay(100);
            if (_displayMessageTask.Task.Status != TaskStatus.RanToCompletion)
            {
                _displayMessageTask.SetResult(true);
            }
        }

        public void DisplayDebugMessage(string message) { }

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