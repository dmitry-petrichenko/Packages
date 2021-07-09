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
        
        public SocketSubstitutionStateAwaitor(
            SocketSubstitution socketSubstitution,
            Counter counter,
            int aimedValue)
        {
            _socketSubstitution = socketSubstitution;
            _counter = counter;
            _aimedValue = aimedValue;
            _taskCompletionSource = new TaskCompletionSource<bool>();

            _socketSubstitution.Updated += UpdatedHandler;
            
            Task = _taskCompletionSource.Task;
        }

        private void UpdatedHandler(
            SocketSubstitution socketSubstitution, 
            ExceptionLine exceptionLine)
        {
            if (_counter.Value == _aimedValue)
            {
                _socketSubstitution.Updated -= UpdatedHandler;
                _taskCompletionSource.SetResult(true);
            }
        }

        public Task<bool> Task { get; }
    }
}