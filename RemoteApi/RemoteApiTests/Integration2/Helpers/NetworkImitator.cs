using System;
using System.Threading.Tasks;

namespace RemoteApi.Integration2.Helpers
{
    public class NetworkImitator
    {
        private readonly SocketTester _socketConnecter;
        private readonly SocketTester _socketAccepted;

        private Task _connecterRaiseTask;
        private Task _acceptedRaiseTask;
        
        public NetworkImitator(SocketTester socketConnecter, SocketTester socketAccepted)
        {
            _socketConnecter = socketConnecter;
            _socketAccepted = socketAccepted;
            
            _connecterRaiseTask = Task.CompletedTask;
            _acceptedRaiseTask = Task.CompletedTask;

            _socketConnecter.SendCalled += SocketConnecterCalledHandler;
            _socketAccepted.SendCalled += SocketAcceptedCalledHandler;
        }

        public void SocketConnecterCalledHandler(byte[] bytes)
        {
            if (_connecterRaiseTask == default)
            {
                throw new Exception("");
            }
            
            if (_connecterRaiseTask.Status != TaskStatus.RanToCompletion)
            {
                _connecterRaiseTask = _connecterRaiseTask.ContinueWith( t =>
                {
                    _socketAccepted.RaiseReceived(bytes);
                });
            }
            else
            {
                _connecterRaiseTask = Task.Run(() =>
                {
                    _socketAccepted.RaiseReceived(bytes);
                });
            }
        }
        
        private void SocketAcceptedCalledHandler(byte[] bytes)
        {
            if (_acceptedRaiseTask == default)
            {
                throw new Exception("");
            }
            
            if (_acceptedRaiseTask.Status != TaskStatus.RanToCompletion)
            {
                _acceptedRaiseTask = _acceptedRaiseTask.ContinueWith( t =>
                {
                    _socketConnecter.RaiseReceived(bytes);
                });
            }
            else
            {
                _acceptedRaiseTask = Task.Run(() =>
                {
                    _socketConnecter.RaiseReceived(bytes);
                });
            }
        }
    }
}