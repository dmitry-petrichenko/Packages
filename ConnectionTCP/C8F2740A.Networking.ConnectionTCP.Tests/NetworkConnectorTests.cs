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
        private ISocketFactory _socketFactory;
        
        public NetworkConnectorTests()
        {
            _socketFactory = Mock.Create<ISocketFactory>();
            _sut = new NetworkConnector(socket =>
            {
                return Mock.Create<INetworkTunnel>();
            }, _socketFactory);
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldCreateSocket()
        {
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            Mock.Arrange(() => _socketFactory.Create(default, default, default))
                .IgnoreArguments()
                .Returns(Mock.Create<ISocket>());
            _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Mock.Assert(() => _socketFactory.Create(
                Arg.IsAny<AddressFamily>(), 
                Arg.IsAny<SocketType>(),  
                Arg.IsAny<ProtocolType>()), Occurs.Once());
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldCallSocketConnect()
        {
            var socket = Mock.Create<ISocket>();
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            Mock.Arrange(() => _socketFactory.Create(default, default, default))
                .IgnoreArguments()
                .Returns(socket);
            _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Mock.Assert(() => socket.Connect(Arg.IsAny<IPAddress>(), Arg.AnyInt), Occurs.Once());
        }
        
        [Fact]
        public void TryConnect_WithNetworkAddress_ShouldReturnTrue()
        {
            var socket = Mock.Create<ISocket>();
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            Mock.Arrange(() => _socketFactory.Create(default, default, default))
                .IgnoreArguments()
                .Returns(socket);
            var result = _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Assert.True(result);
        }
        
        [Fact]
        public void TryConnect_OnSocketException_ShouldReturnFalse()
        {
            var socket = Mock.Create<ISocket>();
            Mock.Arrange(() => socket.Connect(null, 0)).IgnoreArguments().Throws<Exception>();
            var networkAddress = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => networkAddress.IP).Returns(IPAddress.Parse("192.168.0.1"));
            Mock.Arrange(() => _socketFactory.Create(default, default, default))
                .IgnoreArguments()
                .Returns(socket);
            var result = _sut.TryConnect(networkAddress, out INetworkTunnel tunnel);
            
            Assert.False(result);

        }
    }
}