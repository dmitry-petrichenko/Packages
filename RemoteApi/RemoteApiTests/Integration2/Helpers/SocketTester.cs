using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration2.Helpers
{
    public class SocketTester : ISocket
    {
        public Action<IPAddress, int> ConnectAction { get; set; }

        public string Tag { get; }
        public int ListenCalledTimes { get; private set; }
        public int ConnectCalledTimes { get; private set; }
        public int ReceiveCalledTimes { get; private set; }
        public int DisposeCalledTimes { get; private set; }

        public event Action<byte[]> SendCalled;
        public event Action<IPAddress, int> ConnectCalled;
        public event Action Byte49ReceivedFirstTime;
        
        private TaskCompletionSource<ISocket> _socketAcceptedTask;
        private BlockingCollection<byte[]> _messageQueue;
        private bool _byte49ReceivedFirstTime;
        
        public SocketTester(string tag = "")
        {
            Tag = tag;
            
            LocalEndPoint = new IPEndPoint(0, 0);
            RemoteEndPoint = new IPEndPoint(0, 0);
            
            _messageQueue = new BlockingCollection<byte[]>();
            _byte49ReceivedFirstTime = false;
        }

        public void SetRemoteEndPoint(IPAddress ipAddress, int port)
        {
            RemoteEndPoint = new IPEndPoint(ipAddress, port);
        }
        
        public void SetLocalEndPoint(IPAddress ipAddress, int port)
        {
            LocalEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void Dispose()
        {
            DisposeCalledTimes++;
        }

        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool Connected { get; set; }
        
        public void Bind(IPAddress ipAddress, int port)
        {
            LocalEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            ConnectCalledTimes++;
            if (ConnectAction != null)
            {
                ConnectAction.Invoke(ipAddress, port);
                return;
            }
            
            Connected = true;
            RemoteEndPoint = new IPEndPoint(ipAddress, port);
            ConnectCalled?.Invoke(ipAddress, port);
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
            
            if (received.Length > 0 && received[0].Equals(49))
            {
                _byte49ReceivedFirstTime = true;
                Byte49ReceivedFirstTime?.Invoke();
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
            _messageQueue.Add(Array.Empty<byte>());
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