using System;
using System.Threading.Tasks;

namespace C8F2740A.NetworkNode.RemoteApi.Monitor
{
    public interface IRemoteTraceMonitorСonsistent : IMutableRemoteTraceMonitor
    {
        event Func<string, Task<bool>> CommandReceived;
    }
    
    public class RemoteTraceMonitorСonsistent : IRemoteTraceMonitorСonsistent
    {
        private readonly IRemoteTraceMonitor _remoteTraceMonitor;
        
        private Task _commandExecutingTask;
        
        public RemoteTraceMonitorСonsistent(IRemoteTraceMonitor remoteTraceMonitor)
        {
            _remoteTraceMonitor = remoteTraceMonitor;
            _commandExecutingTask = Task.CompletedTask;

            _remoteTraceMonitor.TextEntered += TextEnteredHandler;
        }

        #region MutableRemoteTraceMonitor
        public void Start() => _remoteTraceMonitor.Start();

        public void Stop() => _remoteTraceMonitor.Stop();

        public void DisplayNextMessage(string message) => _remoteTraceMonitor.DisplayNextMessage(message);

        public void DisplayDebugMessage(string message) => _remoteTraceMonitor.DisplayDebugMessage(message);

        public void ClearTextBox() => _remoteTraceMonitor.ClearTextBox();

        public void SetPrompt(string value) => _remoteTraceMonitor.SetPrompt(value);
        #endregion
        
        public event Func<string, Task<bool>> CommandReceived;

        private void TextEnteredHandler(string value)
        {
            if (_commandExecutingTask == default)
            {
                throw new Exception("Task must be initialized");
            }
            
            if (_commandExecutingTask.Status != TaskStatus.RanToCompletion)
            {
                _commandExecutingTask = _commandExecutingTask.ContinueWith( t =>
                {
                    CommandReceived?.Invoke(value).Wait();
                });
            }
            else
            {
                _commandExecutingTask = Task.Run(() =>
                {
                    CommandReceived?.Invoke(value).Wait();
                });
            }
        }
    }
}