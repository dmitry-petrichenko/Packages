using System.Net;
using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests
{
    public class NetworkTunnelTests
    {
        private INetworkTunnel _sut;
        private ISocket _socket;
        private IRecorder _recorder;

        public NetworkTunnelTests()
        {
            _socket = Mock.Create<ISocket>();
            _recorder = Mock.Create<IRecorder>();
            _sut = new NetworkTunnel(_socket, _recorder);
        }
        
        [Fact]
        public void ConstructorCall_WithParameters_ShouldRecordMessage()
        {
            _recorder = Mock.Create<IRecorder>();
            _sut = new NetworkTunnel(_socket, _recorder);
            
            Mock.Assert(() => _recorder.RecordInfo(Arg.AnyString, Arg.AnyString), Occurs.Exactly(2));
        }
        
        [Fact]
        public void Send_WithParameters_ShouldSentWithSameParameters()
        {
            var paramToSent = new byte[] { 0b01010101 };
            
            _sut.Send(paramToSent);
            
            Mock.Assert(() => _socket.Send(paramToSent), Occurs.Once());
        }
        
        [Fact]
        public void Dispose_CalledTwice_ShouldCallSocketDisposeOnce()
        {
            _sut.Dispose();
            _sut.Dispose();
            
            Mock.Assert(() => _socket.Dispose(), Occurs.Once());
        }
        
        [Fact]
        public void Dispose_CalledTwice_ShouldCallClosedEventOnce()
        {
            var closeRaised = 0;
            var socket = Mock.Create<ISocket>();
            Mock.Arrange(() => socket.Connected).Returns(true);
            _sut = new NetworkTunnel(socket, _recorder);
            
            _sut.Closed += () => closeRaised++;
            
            _sut.Dispose();
            _sut.Dispose();
            
            Assert.Equal(1, closeRaised);
        }
    }
}