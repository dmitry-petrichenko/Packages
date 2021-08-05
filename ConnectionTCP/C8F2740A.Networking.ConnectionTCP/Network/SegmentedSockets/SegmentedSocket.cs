using System.Net;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets
{
    public interface ISegmentedSocket 
    {
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        bool Connected { get; }
        string Tag { get; }

        void Bind(IPAddress ipAddress, int port);
        void Connect(IPAddress ipAddress, int port);
        void Listen(int backlog);
        void Send(byte[] data);
        (int, byte[]) Receive();
        Task<ISegmentedSocket> AcceptAsync();
        void Dispose();
    }
    
    public class SegmentedSocket : ISegmentedSocket
    {
        private readonly IDataSplitter _dataSplitter;
        private readonly IDataSplitterFactory _dataSplitterFactory;
        private readonly ISocket _socket;
        
        public SegmentedSocket(
            ISocket socket, 
            IDataSplitterFactory dataSplitterFactory)
        {
            _socket = socket;
            _dataSplitterFactory = dataSplitterFactory;
            _dataSplitter = _dataSplitterFactory.Create(_socket);
        }

        public void Dispose()
        {
            _dataSplitter.Dispose();
            _socket.Dispose();
        }

        public IPEndPoint LocalEndPoint => _socket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => _socket.RemoteEndPoint;
        public bool Connected => _socket.Connected;
        public string Tag => _socket.Tag;
        public void Bind(IPAddress ipAddress, int port)
        {
            _socket.Bind(ipAddress, port);
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            _socket.Connect(ipAddress, port);
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        public void Send(byte[] data)
        {
            _dataSplitter.Send(data);
        }

        public (int, byte[]) Receive()
        {
            var received = _dataSplitter.Receive();
            
            return (received.Length, received);
        }

        public async Task<ISegmentedSocket> AcceptAsync()
        {
            var socket = await _socket.AcceptAsync();
            var messageSplitter = new SegmentedSocket(socket, _dataSplitterFactory);
            
            return messageSplitter;
        }
    }
}