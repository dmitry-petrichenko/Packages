using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace C8F2740A.NetworkNode.SessionTCP
{
    internal interface ISessionHolder
    {
        void Set(ISession session);
        void Clear();
        Task<(bool, IEnumerable<byte>)> SendInstruction(IEnumerable<byte> instruction);
        event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
        
        bool HasActiveSession { get; }
    }
    
    internal class SessionHolder : ISessionHolder
    {
        public bool HasActiveSession { get; private set; }
        
        private ISession _currentSession;
        private bool _hasActiveSession;
        private TaskCompletionSource<IEnumerable<byte>> _sendInstructionTask;

        public SessionHolder()
        {
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
            ValidateSend();
            _sendInstructionTask = new TaskCompletionSource<IEnumerable<byte>>();
            
            _currentSession.Send(instruction);
            var response =  await _sendInstructionTask.Task;
            
            return (true, response);
        }

        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;

        private void ClosedHandler()
        {
            _currentSession.Received -= ReceivedHandler;
            _currentSession.Responded -= RespondedHandler;
            _currentSession.Closed -= ClosedHandler;

            _currentSession = default;
            HasActiveSession = false;
        }

        private void RespondedHandler(IEnumerable<byte> value)
        {
            _sendInstructionTask.SetResult(value);
        }

        private void ReceivedHandler(IEnumerable<byte> value)
        {
            var result = InstructionReceived.Invoke(value);
            
            if (_currentSession != default)
            {
                _currentSession.Response(result);
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
    }
}