using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests
{
    public class NetworkTunnelTests
    {
        private INetworkTunnel _sut;
        private ISegmentedSocket _socket;
        private IRecorder _recorder;

        public NetworkTunnelTests()
        {
            _socket = Mock.Create<ISegmentedSocket>();
            _recorder = Mock.Create<IRecorder>();
        }
        
        [Fact]
        public void ConstructorCall_WithParameters_ShouldRecordMessage()
        {
            _recorder = Mock.Create<IRecorder>();
            _sut = new NetworkTunnel(_socket, _recorder);
            
            Mock.Assert(() => _recorder.RecordInfo(Arg.AnyString, Arg.AnyString), Occurs.Exactly(1));
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
        public void Send_ThrowsSocketException_ShouldDisconnect()
        {
            bool calledDisconnected = false;
            var paramToSent = new byte[] { 0b01010101 };
            Mock.Arrange(() => _socket.Send(null)).IgnoreArguments().Throws(new SocketException());
            _sut = new NetworkTunnel(_socket, _recorder);
            _sut.Disconnected += () => calledDisconnected = true;

            _sut.Send(paramToSent);

            Assert.True(calledDisconnected);
            //Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Send_ThrowsNoSocketException_ShouldBeCaught()
        {
            var paramToSent = new byte[] { 0b01010101 };
            Mock.Arrange(() => _socket.Send(null)).IgnoreArguments().Throws(new Exception());
            _sut = new NetworkTunnel(_socket, _recorder);

            _sut.Send(paramToSent);

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }

        [Fact]
        public void Dispose_WhenCalled_ShouldCallSocketDispose()
        {
            var socket = Mock.Create<ISegmentedSocket>();
            _sut = new NetworkTunnel(socket, _recorder);

            _sut.Dispose();

            Mock.Assert(() => socket.Dispose(), Occurs.Once());
        }
        
        [Fact]
        public async Task SocketReceive_ThrowsSocketExteption_ShouldRaiseDisconnected()
        {
            bool disconnected = false;
            var socket = Mock.Create<ISegmentedSocket>();
            Mock.Arrange(() => socket.Receive()).IgnoreArguments().Throws(new SocketException());
            Mock.Arrange(() => socket.Connected).Returns(true);

            _sut = new NetworkTunnel(socket, _recorder);
            _sut.Disconnected += () => disconnected = true;
            _sut.Listen();
            await Task.Delay(100);
            Assert.True(disconnected);
        }
    }
}