using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using SocketSubstitutionTests;

namespace RemoteApi.Integration.Helpers
{
    public class SocketSubstitution : ISocket
    {
        public string Tag { get; }
        
        public Counter AcceptAsyncCalledTimes { get; private set; }
        public Counter CloseCalledTimes { get; private set; }
        public Counter ReceiveCalledTimes { get; private set; }
        public Counter SendCalledTimes { get; private set; }
        public Counter ListenCalledTimes { get; private set; }
        public Counter ConnectCalledTimes { get; private set; }
        public Counter BindCalledTimes { get; private set; }
        public Counter DisposeCalledTimes { get; private set; }
        
        public IPEndPoint LocalEndPoint => _socket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => _socket.RemoteEndPoint;
        public bool Connected => _socket.Connected;
        
        public event Action<SocketSubstitution, ExceptionLine, string> UpdatedAfter;
        public event Action<SocketSubstitution, ExceptionLine, string> UpdatedBefore;

        private ISocket _socket;
        private Func<AddressFamily, SocketType, ProtocolType, ISocket> _factory;
        private Func<ISocket, string, ISocket> _acceptFactory;
        

        public SocketSubstitution(
            Func<AddressFamily, SocketType, ProtocolType, ISocket> factory,
            Func<ISocket, string, ISocket> acceptFactory,
            AddressFamily addressFamily, 
            SocketType socketType, 
            ProtocolType protocolType, 
            string tag)
        {
            Tag = tag;
            _factory = factory;
            _acceptFactory = acceptFactory;
            _socket = _factory.Invoke(addressFamily, socketType, protocolType);
            Initialize();
        }

        public SocketSubstitution(ISocket socket, string tag)
        {
            Tag = tag;
            _socket = socket;
            Initialize();
        }

        private void Initialize()
        {
            AcceptAsyncCalledTimes = new Counter();
            CloseCalledTimes = new Counter();
            ReceiveCalledTimes = new Counter();
            SendCalledTimes = new Counter();
            ListenCalledTimes = new Counter();
            ConnectCalledTimes = new Counter();
            BindCalledTimes = new Counter();
            DisposeCalledTimes = new Counter();
        }

        public void Dispose()
        {
            UpdateBeforeInternal("Dispose");
            _socket.Dispose();
            DisposeCalledTimes.Tick();
            UpdateAfterInternal("Dispose");
        }

        public void Bind(IPAddress ipAddress, int port)
        {
            UpdateBeforeInternal("Bind");
            _socket.Bind(ipAddress, port);
            BindCalledTimes.Tick();
            UpdateAfterInternal("Bind");
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            UpdateBeforeInternal("Connect");
            _socket.Connect(ipAddress, port);
            ConnectCalledTimes.Tick();
            UpdateAfterInternal("Connect");
        }

        public void Listen(int backlog)
        {
            UpdateBeforeInternal("Listen");
            _socket.Listen(backlog);
            ListenCalledTimes.Tick();
            UpdateAfterInternal("Listen");
        }

        public void Send(byte[] data)
        {
            UpdateBeforeInternal("Send");
            _socket.Send(data);
            SendCalledTimes.Tick();
            UpdateAfterInternal("Send");
        }

        public int Receive(byte[] bytes)
        {
            int receivedValue = _socket.Receive(bytes);
            ReceiveCalledTimes.Tick();
            UpdateAfterInternal("Receive");
            
            return receivedValue;
        }

        public async Task<ISocket> AcceptAsync()
        {
            var socket = await _socket.AcceptAsync();
            AcceptAsyncCalledTimes.Tick();
            var wrapper = _acceptFactory.Invoke(socket, $"{Tag}:accept_{AcceptAsyncCalledTimes.Value}");
            UpdateAfterInternal("AcceptAsync");
            
            return wrapper;
        }

        public void Close()
        {
            UpdateBeforeInternal("Close");
            _socket.Close();
            CloseCalledTimes.Tick();
            UpdateAfterInternal("Close");
        }
        
        private void UpdateBeforeInternal(string methodName)
        {
            ExceptionLine exceptionLine = new ExceptionLine();
            UpdatedBefore?.Invoke(this, exceptionLine, methodName);
            exceptionLine.Value.Invoke();
        }

        private void UpdateAfterInternal(string methodName)
        {
            ExceptionLine exceptionLine = new ExceptionLine();
            UpdatedAfter?.Invoke(this, exceptionLine, methodName);
            exceptionLine.Value.Invoke();
        }
    }
}

