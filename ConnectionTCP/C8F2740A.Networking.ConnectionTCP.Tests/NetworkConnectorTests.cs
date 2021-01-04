using System;
using System.Net;
using System.Net.Sockets;
using C8F2740A.Networking.ConnectionTCP.Network;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests
{
    public class NetworkConnectorTests
    {
        private INetworkConnector _sut;
        private Func<AddressFamily, SocketType, ProtocolType, ISocket> _socketFactory;
        
        public NetworkConnectorTests()
        {
            _socketFactory = (a, s, p) => Mock.Create<ISocket>();
            _sut = new NetworkConnector(socket => Mock.Create<INetworkTunnel>(), 
                _socketFactory);
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldCreateSocket()
        {
            var wasCalledTimes = 0;
            _socketFactory = (a, s, p) =>
            {
                wasCalledTimes++;
                return Mock.Create<ISocket>();
            };
            _sut = new NetworkConnector(socket => Mock.Create<INetworkTunnel>(), 
                _socketFactory);
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            
            _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Assert.Equal(1, wasCalledTimes);
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldCallSocketConnect()
        {
            var socket = Mock.Create<ISocket>();
            _socketFactory = (a, s, p) => socket;
            _sut = new NetworkConnector(socket => Mock.Create<INetworkTunnel>(), 
                _socketFactory);
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Mock.Assert(() => socket.Connect(Arg.IsAny<IPAddress>(), Arg.AnyInt), Occurs.Once());
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldReturnTrue()
        {
            var socket = Mock.Create<ISocket>();
            _socketFactory = (a, s, p) => socket;
            _sut = new NetworkConnector(socket => Mock.Create<INetworkTunnel>(), 
                _socketFactory);
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            
            var result = _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Assert.True(result);
        }
        
        [Fact]
        public void TryConnect_OnSocketException_ShouldReturnFalse()
        {
            var socket = Mock.Create<ISocket>();
            _socketFactory = (a, s, p) => socket;
            _sut = new NetworkConnector(socket => Mock.Create<INetworkTunnel>(), 
                _socketFactory);
            Mock.Arrange(() => socket.Connect(null, 0)).IgnoreArguments().Throws<Exception>();
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));

            var result = _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Assert.False(result);

        }
    }
}