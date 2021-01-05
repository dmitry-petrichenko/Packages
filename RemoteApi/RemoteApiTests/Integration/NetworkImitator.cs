namespace RemoteApi.Integration
{
    public class NetworkImitator
    {
        private readonly SocketTester _socketTester1;
        private readonly SocketTester _socketTester2;
        
        public NetworkImitator(SocketTester socketTester1, SocketTester socketTester2)
        {
            _socketTester1 = socketTester1;
            _socketTester2 = socketTester2;

            _socketTester1.SendCalled += Socket1SendCalledHandler;
        }

        private void Socket1SendCalledHandler(byte[] bytes)
        {
            _socketTester2.RaiseReceived(bytes);
        }
    }
}