using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration.Helpers
{
    public class SocketTesterWrapper : ISocket
    {
        public int CloseCalledTimes { get; private set; }
        public string Tag { get; }

        public event Action<SocketTesterWrapper> Accepted;

        private Socket _socket;

        public SocketTesterWrapper(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            Tag = tag;
            _socket = new Socket(addressFamily, socketType, protocolType);
        }
        
        public SocketTesterWrapper(Socket socket, string tag)
        {
            Tag = tag;
            _socket = socket;
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)_socket.RemoteEndPoint;
        public bool Connected => _socket.Connected;

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
    }
}