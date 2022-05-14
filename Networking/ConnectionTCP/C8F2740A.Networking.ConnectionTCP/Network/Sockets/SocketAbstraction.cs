using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace C8F2740A.Networking.ConnectionTCP.Network.Sockets
{
    public class SocketAbstraction : ISocket
    {
        private Socket _socket;

        public SocketAbstraction(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
        {
            Tag = tag;
            _socket = new Socket(addressFamily, socketType, protocolType);
        }
        
        public SocketAbstraction(Socket socket)
        {
            _socket = socket;
        }

        public void Dispose()
        {
            if (_socket.Connected)
            {
                try
                {
                    _socket.Disconnect(false);
                }
                catch (Exception e) { }
            }

            _socket.Dispose();
        }

        public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)_socket.RemoteEndPoint;
        public bool Connected => _socket.Connected;
        public string Tag { get; }

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
            return _socket.Receive(bytes);
        }

        public async Task<ISocket> AcceptAsync()
        {
            var socket = await _socket.AcceptAsync();
            
            return new SocketAbstraction(socket);
        }
    }
}