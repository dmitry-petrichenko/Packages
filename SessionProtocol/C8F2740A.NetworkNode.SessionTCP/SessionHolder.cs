using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface ISessionHolder : IDisposable
    {
        void Set(ISession session);
        void Clear();
        Task<(bool, IEnumerable<byte>)> SendInstruction(IEnumerable<byte> instruction);
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        
        bool HasActiveSession { get; }
    }
    
    public class SessionHolder : ISessionHolder
    {
        public bool HasActiveSession { get; private set; }
        
        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;

        private readonly IRecorder _recorder;
        
        private ISession _currentSession;
        private TaskCompletionSource<IEnumerable<byte>> _sendInstructionTask;

        public SessionHolder(IRecorder recorder)
        {
            _recorder = recorder;
            _currentSession = default;
            HasActiveSession = false;
        }

        public void Set(ISession session)
        {
            if (session == default)
            {
                throw new Exception("Session cannot be null");
            }
            
            Clear();
            _currentSession = session;
            _currentSession.Received += ReceivedHandler;
            _currentSession.Responded += RespondedHandler;
            _currentSession.Closed += ClosedHandler;
            HasActiveSession = true;
            _currentSession.Listen();
        }
        
        public void Clear()
        {
            if (_currentSession == default)
            {
                return;
            }
            
            _currentSession.Close();
        }

        public async Task<(bool, IEnumerable<byte>)> SendInstruction(IEnumerable<byte> instruction)
        {
            var result = (false, Enumerable.Empty<byte>());
            try
            {
                result = await SendInstructionInternal(instruction);
            }
            catch (Exception exception)
            {
                _recorder.DefaultException(this, exception);
            }

            return result;
        }
        
        private async Task<(bool, IEnumerable<byte>)> SendInstructionInternal(IEnumerable<byte> instruction)
        {
            ValidateSend();
            _sendInstructionTask = new TaskCompletionSource<IEnumerable<byte>>();
            
            _currentSession.Send(instruction);
            var response =  await _sendInstructionTask.Task;
            
            return (true, response);
        }

        private void ClosedHandler()
        {
            ClearAndReset();
        }

        private void ClearAndReset()
        {
            _currentSession.Received -= ReceivedHandler;
            _currentSession.Responded -= RespondedHandler;
            _currentSession.Closed -= ClosedHandler;

            _currentSession = default;
            HasActiveSession = false;
            
            if (_sendInstructionTask != default)
            {
                _sendInstructionTask.TrySetCanceled();
                _sendInstructionTask = default;
            }
        }

        private void RespondedHandler(IEnumerable<byte> value)
        {
            _sendInstructionTask.SetResult(value);
        }

        private void ReceivedHandler(IEnumerable<byte> value)
        {
            var result = Enumerable.Empty<byte>();
            try
            {
                result = InstructionReceived?.Invoke(value);
            }
            catch (Exception exception)
            {
                _recorder.DefaultException(this, exception);
            }
            
            try
            {
                if (_currentSession != default)
                {
                    _currentSession.Response(result);
                }
            }
            catch (Exception exception)
            {
                _recorder.DefaultException(this, exception);
            }
        }

        private void ValidateSend()
        {
            if (_sendInstructionTask != default && !_sendInstructionTask.Task.IsCompleted)
            {
                throw new Exception("Attempt to send while task is not completed");
            }
            
            if (_currentSession == default)
            {
                throw new Exception("Attempt to send when session is null");
            }
        }

        public void Dispose()
        {
            ClearAndReset();
        }
    }
}