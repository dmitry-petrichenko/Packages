using System.Net;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration
{
    public class SocketTester : ISocket
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public bool Connected { get; }
        public void Bind(IPAddress ipAddress, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Listen(int backlog)
        {
            throw new System.NotImplementedException();
        }

        public void Send(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public int Receive(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public Task<ISocket> AcceptAsync()
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }
    }
}