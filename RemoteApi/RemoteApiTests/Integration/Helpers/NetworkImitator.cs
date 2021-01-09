using System.Threading.Tasks;

namespace RemoteApi.Integration.Helpers
{
    public class NetworkImitator
    {
        private readonly SocketTester _socketConnecter;
        private readonly SocketTester _socketAccepted;
        
        public NetworkImitator(SocketTester socketConnecter, SocketTester socketAccepted)
        {
            _socketConnecter = socketConnecter;
            _socketAccepted = socketAccepted;

            _socketConnecter.SendCalled += SocketConnecterCalledHandler;
            _socketAccepted.SendCalled += SocketAcceptedCalledHandler;
        }

        private void SocketConnecterCalledHandler(byte[] bytes)
        {
            Task.Run(async () =>
            {
                _socketAccepted.RaiseReceived(bytes);
            });
        }
        
        private void SocketAcceptedCalledHandler(byte[] bytes)
        {
            Task.Run(async () =>
            {
                _socketConnecter.RaiseReceived(bytes);
            });
        }
    }
}