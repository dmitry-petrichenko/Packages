using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration.Helpers
{
    public class SocketTester : ISocket
    {
        public string Tag { get; }
        public int ListenCalledTimes { get; private set; }
        public int ConnectCalledTimes { get; private set; }
        public int ReceiveCalledTimes { get; private set; }

        public event Action<byte[]> SendCalled;
        public event Action ConnectCalled;
        
        private TaskCompletionSource<ISocket> _socketAcceptedTask;
        private BlockingCollection<byte[]> _messageQueue;
        
        public SocketTester(string tag = "")
        {
            Tag = tag;
            
            _messageQueue = new BlockingCollection<byte[]>();
        }

        public void Dispose()
        {
        }

        public IPEndPoint LocalEndPoint => new IPEndPoint(0, 0);
        public IPEndPoint RemoteEndPoint => new IPEndPoint(0, 0);
        public bool Connected { get; set; }
        
        public void Bind(IPAddress ipAddress, int port)
        {
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            Connected = true;
            ConnectCalledTimes++;
            ConnectCalled?.Invoke();
        }

        public void Listen(int backlog)
        {
            ListenCalledTimes++;
        }

        public void Send(byte[] data)
        {
            SendCalled?.Invoke(data);
        }
        
        public int Receive(byte[] bytes)
        {
            ReceiveCalledTimes++;
            var received = _messageQueue.Take();

            if (received.Length > bytes.Length)
            {
                throw new Exception("Exception");
            }

            Buffer.BlockCopy(received, 0, bytes, 0, received.Length);
            
            return received.Length;
        }
        
        public void RaiseReceived(byte[] bytes)
        {
            _messageQueue.Add(bytes);
        }
        
        public void RaiseDisconnected()
        {
            Connected = false;
        }

        public Task<ISocket> AcceptAsync()
        {
            _socketAcceptedTask = new TaskCompletionSource<ISocket>();
            return _socketAcceptedTask.Task;
        }

        public void RaiseSocketAccepted(ISocket socket)
        {
            _socketAcceptedTask.SetResult(socket);
        }

        public void Close()
        {
        }
    }
}