using System.Threading.Tasks;

namespace RemoteApi.Integration.Helpers.SocketsSubstitution
{
    internal class SocketSubstitutionStateAwaitor
    {
        private TaskCompletionSource<bool> _taskCompletionSource;
        private int _aimedValue;
        
        private readonly SocketSubstitution _socketSubstitution;
        private readonly Counter _counter;
        private readonly int _timeoutTime;
        
        public SocketSubstitutionStateAwaitor(
            SocketSubstitution socketSubstitution,
            Counter counter,
            int aimedValue,
            int timeoutTime)
        {
            _socketSubstitution = socketSubstitution;
            _counter = counter;
            _aimedValue = aimedValue;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            _timeoutTime = timeoutTime;

            _socketSubstitution.UpdatedAfter += UpdatedAfterHandler;

            StartTimeout();
            Task = _taskCompletionSource.Task;

            UpdatedAfterHandler(_socketSubstitution, new ExceptionLine(), "");
        }

        private async void StartTimeout()
        {
            await System.Threading.Tasks.Task.Delay(_timeoutTime);
            System.Threading.Tasks.Task.Run(() => _taskCompletionSource.TrySetResult(false));
        }

        private void UpdatedAfterHandler(
            SocketSubstitution socketSubstitution, 
            ExceptionLine exceptionLine,
            string tag)
        {
            if (_counter.Value == _aimedValue)
            {
                _socketSubstitution.UpdatedAfter -= UpdatedAfterHandler;
                System.Threading.Tasks.Task.Run(() => _taskCompletionSource.TrySetResult(true));
            }
        }

        public Task<bool> Task { get; }
    }
}