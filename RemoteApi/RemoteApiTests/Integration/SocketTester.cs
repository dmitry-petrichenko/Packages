using System;
using System.Net;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration
{
    public class SocketTester : ISocket
    {
        public event Action<byte[]> SendCalled;

        private TaskCompletionSource<byte[]> _source;

        public void Dispose()
        {
        }

        public IPEndPoint LocalEndPoint => new IPEndPoint(0, 0);
        public IPEndPoint RemoteEndPoint => new IPEndPoint(0, 0);
        public bool Connected { get; private set; }
        
        public void Bind(IPAddress ipAddress, int port)
        {
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            Connected = true;
        }

        public void Listen(int backlog)
        {
        }

        public void Send(byte[] data)
        {
            SendCalled?.Invoke(data);
        }

        public int Receive(byte[] bytes)
        {
            _source = new TaskCompletionSource<byte[]>();
            _source.Task.Wait();
            var received = _source.Task.Result;

            if (received.Length > bytes.Length)
            {
                throw new Exception("Exception");
            }

            Buffer.BlockCopy(received, 0, bytes, 0, received.Length);
            
            return received.Length;
        }

        public void RaiseReceived(byte[] bytes)
        {
            _source?.SetResult(bytes);
        }
        
        public void RaiseDisconnected()
        {
            Connected = false;
            _source?.SetResult(Array.Empty<byte>());
        }

        public Task<ISocket> AcceptAsync()
        {
            return new TaskCompletionSource<ISocket>().Task;
        }

        public void Close()
        {
        }
    }
}