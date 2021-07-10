using System;
using RemoteApi.Integration.Helpers;

namespace SocketSubstitutionTests
{
    internal class SocketSubstitutionStateHandler
    {
        private Action _handler;
        private int _aimedValue;
        
        private readonly SocketSubstitution _socketSubstitution;
        private readonly Counter _counter;
        
        public SocketSubstitutionStateHandler(
            SocketSubstitution socketSubstitution,
            Counter counter,
            Action handler,
            int aimedValue)
        {
            _socketSubstitution = socketSubstitution;
            _counter = counter;
            _aimedValue = aimedValue;
            _handler = handler;

            _socketSubstitution.Updated += UpdatedHandler;

        }

        private void UpdatedHandler(
            SocketSubstitution socketSubstitution, 
            ExceptionLine exceptionLine)
        {
            if (_counter.Value == _aimedValue)
            {
                _socketSubstitution.Updated -= UpdatedHandler;
                _handler?.Invoke();
            }
        }
    }
}