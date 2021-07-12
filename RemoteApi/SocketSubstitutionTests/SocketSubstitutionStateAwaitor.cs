using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;

namespace SocketSubstitutionTests
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

            _socketSubstitution.Updated += UpdatedHandler;

            StartTimeout();
            Task = _taskCompletionSource.Task;

            UpdatedHandler(_socketSubstitution, new ExceptionLine());
        }

        private async void StartTimeout()
        {
            await System.Threading.Tasks.Task.Delay(_timeoutTime);
            System.Threading.Tasks.Task.Run(() => _taskCompletionSource.TrySetResult(false));
        }

        private void UpdatedHandler(
            SocketSubstitution socketSubstitution, 
            ExceptionLine exceptionLine)
        {
            if (_counter.Value == _aimedValue)
            {
                _socketSubstitution.Updated -= UpdatedHandler;
                System.Threading.Tasks.Task.Run(() => _taskCompletionSource.TrySetResult(true));
            }
        }

        public Task<bool> Task { get; }
    }
}