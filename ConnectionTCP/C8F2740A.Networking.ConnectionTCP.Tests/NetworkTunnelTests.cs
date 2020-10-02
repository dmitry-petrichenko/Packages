using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using Telerik.JustMock;
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
            _sut = new NetworkTunnel(_socket, _recorder);
            
            _sut.Send(paramToSent);
            
            Mock.Assert(() => _socket.Send(paramToSent), Occurs.Once());
        }
        
        [Fact]
        public void Send_Throws_ShouldBeCaught()
        {
            var paramToSent = new byte[] { 0b01010101 };
            Mock.Arrange(() => _socket.Send(null)).IgnoreArguments().Throws(new SocketException());
            _sut = new NetworkTunnel(_socket, _recorder);
            
            _sut.Send(paramToSent);

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Send_ThrowsInterruptConnection_ShouldBeCaught()
        {
            var paramToSent = new byte[] { 0b01010101 };
            Mock.Arrange(() => _socket.Send(null)).IgnoreArguments().Throws(new SocketException(10054));
            _sut = new NetworkTunnel(_socket, _recorder);
            
            _sut.Send(paramToSent);

            Mock.Assert(() => _socket.Dispose(), Occurs.Exactly(2));
            Mock.Assert(() => _recorder.RecordInfo(Arg.AnyString, Arg.AnyString), Occurs.Exactly(3));
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Never());
        }
        
        [Fact]
        public void Dispose_CalledTwice_ShouldCallSocketDisposeOnce()
        {
            var socket = Mock.Create<ISocket>();
            Mock.Arrange(() => socket.Connected).Returns(true);
            _sut = new NetworkTunnel(socket, _recorder);
            
            _sut.Dispose();
            _sut.Dispose();
            
            Mock.Assert(() => socket.Dispose(), Occurs.Once());
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
        
        [Fact]
        public async Task SocketReceive_Throws_ShouldBeCaught()
        {
            var socket = Mock.Create<ISocket>();
            Mock.Arrange(() => socket.Receive(null)).IgnoreArguments().Throws(new SocketException());
            Mock.Arrange(() => socket.Connected).Returns(true);

            _sut = new NetworkTunnel(socket, _recorder);
            await Task.Delay(100);
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
    }
}