using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteApi.Integration.Helpers
{
    internal static class CacheRecorderExtensions
    {
        public static Task<bool> ArrangeWaitingMessage(
            this ApplicationCacheRecorder applicationCacheRecorder, 
            string text,
            int timeout)
        {
            return new MessageAwaiter(applicationCacheRecorder, text, timeout).WaitingTask;
        }
    }

    internal class MessageAwaiter
    {
        private TaskCompletionSource<bool> _taskCompletionSource;
        
        private readonly ApplicationCacheRecorder _applicationCacheRecorder;
        private readonly string _expectedMessage;
        private readonly int _timeoutTime;

        public Task<bool> WaitingTask { get; }

        public MessageAwaiter(
            ApplicationCacheRecorder applicationCacheRecorder,
            string expectedMessage,
            int timeoutTime)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            WaitingTask = _taskCompletionSource.Task;

            _applicationCacheRecorder = applicationCacheRecorder;
            _expectedMessage = expectedMessage;
            _timeoutTime = timeoutTime;
            
            _applicationCacheRecorder.DisplayMessageCalledWithText += Handler;
            
            StartTimeout();
        }

        private void Handler(string text)
        {
            _applicationCacheRecorder.DisplayMessageCalledWithText -= Handler;
            
            if (Regex.IsMatch(text, _expectedMessage))
            {
                _taskCompletionSource.TrySetResult(true);
            }
        }

        private async void StartTimeout()
        {
            await System.Threading.Tasks.Task.Delay(_timeoutTime);
            Task.Run(() => _taskCompletionSource.TrySetResult(false));
        }
    }
}