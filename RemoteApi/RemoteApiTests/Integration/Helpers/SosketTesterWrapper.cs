using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace RemoteApi.Integration.Helpers
{
    public class SocketTesterWrapper : ISocket
    {
        public int CloseCalledTimes { get; private set; }
        public int DisposeCalledTimes { get; private set; }
        public int ReceiveCalledTimes { get; private set; }
        public string Tag { get; }
        public Task Disposed => _socketDisposeTask.Task;
        public Task ReceivedCalledSecondTime => _socketConnectedTask.Task;
        public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)_socket.RemoteEndPoint;
        public bool Connected => _socket.Connected;

        public event Action<SocketTesterWrapper> Accepted;

        private Socket _socket;
        private TaskCompletionSource<bool> _socketDisposeTask;
        private TaskCompletionSource<bool> _socketConnectedTask;

        public SocketTesterWrapper(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            Tag = tag;
            _socketDisposeTask = new TaskCompletionSource<bool>();
            _socketConnectedTask = new TaskCompletionSource<bool>();
            _socket = new Socket(addressFamily, socketType, protocolType);

            ConnectCheck();
        }

        public SocketTesterWrapper(Socket socket, string tag)
        {
            _socketDisposeTask = new TaskCompletionSource<bool>();
            _socketConnectedTask = new TaskCompletionSource<bool>();
            Tag = tag;
            _socket = socket;
            
            ConnectCheck();
        }

        public void Dispose()
        {
            DisposeCalledTimes++;
            _socket.Dispose();
            _socketDisposeTask.SetResult(true);
        }

        public void Bind(IPAddress ipAddress, int port)
        {
            _socket.Bind(new IPEndPoint(ipAddress, port));
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            _socket.Connect(new IPEndPoint(ipAddress, port));
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        public void Send(byte[] data)
        {
            _socket.Send(data);
        }

        public int Receive(byte[] bytes)
        {
            int receivedValue = _socket.Receive(bytes);
            ReceiveCalledTimes++;
            return receivedValue;
        }

        public async Task<ISocket> AcceptAsync()
        {
            var socket = await _socket.AcceptAsync();
            var wrapper = new SocketTesterWrapper(socket, "accepted");
            Accepted?.Invoke(wrapper);
            
            return wrapper;
        }

        public void Close()
        {
            CloseCalledTimes++;
            _socket.Close();
        }
        
        private void ConnectCheck()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (ReceiveCalledTimes > 1)
                    {
                        _socketConnectedTask.SetResult(true);
                        break;
                    }
                }
            });
        }
    }
}