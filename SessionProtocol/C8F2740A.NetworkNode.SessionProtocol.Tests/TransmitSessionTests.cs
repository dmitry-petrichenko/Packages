using System;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.Commands;
using NSubstitute;
using Telerik.JustMock;
using Xunit;
using Arg = Telerik.JustMock.Arg;

namespace C8F2740A.NetworkNode.SessionProtocol.Tests
{
    public class TransmitSessionTests
    {
        private ITransmitSession _sut;
        private INetworkConnector _networkConnector;
        private INetworkAddressFactory _networkAddressFactory;
        private IRecorder _recorder;
        
        public TransmitSessionTests()
        {
            _networkConnector = Mock.Create<INetworkConnector>();
            _recorder = Mock.Create<IRecorder>();
            _networkAddressFactory = Mock.Create<INetworkAddressFactory>();
            _sut = new TransmitSession(_networkConnector, _networkAddressFactory, _recorder);
        }

        [Fact]
        public void Authorizate_OnCall_ShouldCallTryConnect()
        {
            _networkConnector = Substitute.For<INetworkConnector>();
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _recorder = Mock.Create<IRecorder>();
            _sut = new TransmitSession(_networkConnector, _networkAddressFactory, _recorder);
            
            _sut.Authorize(() => "", "remote");

            _networkConnector.Received().TryConnect(NSubstitute.Arg.Any<INetworkAddress>(), out INetworkTunnel _);
        }
        
        [Fact]
        public void Authorizate_OnCall_ShouldSubscribeOnOpenedTunnel()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            Mock.ArrangeSet(() => networkTunnel.Received += null).IgnoreArguments().OccursOnce();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "", "remote");
            
            Mock.AssertAll(networkTunnel);
        }
        
        [Fact]
        public void Authorizate_OnCall_ShouldSubscribeOnCloseOpenedTunnel()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            Mock.ArrangeSet(() => networkTunnel.Closed += null).IgnoreArguments().OccursOnce();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "", "remote");
            
            Mock.AssertAll(networkTunnel);
        }
        
        [Fact]
        public void Authorizate_OnFirstCall_ShouldSendLoginInfo()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "a", "remote");

            Mock.Assert(() => networkTunnel.Send(Encoding.ASCII.GetBytes("a")), Occurs.Once());
        }
        
        [Fact]
        public void SendLoginInfo_ThrowsOnCall_ShouldCatchException()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            Mock.Arrange(() => networkTunnel.Send(null)).IgnoreArguments().Throws<Exception>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "a", "remote");

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }

        [Fact]
        public void BytesReceived_OnResponseLoginSuccess_ShouldSetResultTrue()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            
            var result = _sut.Authorize(() => "test", "remote");
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS });

            Assert.True(result.Result);
        }
        
        [Fact]
        public void SendCommand_OnCall_ShouldSendCommandInTunnel()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "", "");
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS });
            var command = new byte[] {0b01010101};
            _sut.SendCommand(command);
            
            Mock.Assert(() => networkTunnel.Send(command), Occurs.Once());
        }
        
        [Fact]
        public void TunnelSendCommand_OnThrows_ShouldCatch()
        {
            var command = new byte[] { 0b01010101 };
            var networkTunnel = Mock.Create<INetworkTunnel>();
            Mock.Arrange(() => networkTunnel.Send(command)).Throws<Exception>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "", "");
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS });

            _sut.SendCommand(command);
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void DataReceivedEvent_OnThrows_ShouldCatch()
        {
            var command = new byte[] { 0b01010101 };
            var networkTunnel = Mock.Create<INetworkTunnel>();
            Mock.Arrange(() => networkTunnel.Send(command)).Throws<Exception>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.DataReceived += bytes => throw new Exception();
            _sut.Authorize(() => "", "");
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS });
            
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)0b01010101 });

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }

        [Fact]
        public async Task BytesReceived_OnAutorizatedResponce_ShouldRaiseDataReceived()
        {
            byte[] receivedData = Array.Empty<byte>();
            var networkTunnel = Mock.Create<INetworkTunnel>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            var tcs = new TaskCompletionSource<bool>();
            _sut.DataReceived += data =>
            {
                receivedData = data;
                tcs.SetResult(true);
            };
            
            _sut.Authorize(() => "test", "remote");
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS });
            
            Mock.Raise(() => networkTunnel.Received += null, new [] { (byte)0b0000_1000 });

            await tcs.Task;
            Assert.Equal(0b0000_1000, receivedData[0]);
        }
        
        [Fact]
        public void Authorizate_TryConnectReturnsFalse_ShouldCallRecordError()
        {
            _networkConnector = Substitute.For<INetworkConnector>();
            _networkConnector.TryConnect(null, out INetworkTunnel _).Returns(false);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _recorder = Mock.Create<IRecorder>();
            _sut = new TransmitSession(_networkConnector, _networkAddressFactory, _recorder);
            
            _sut.Authorize(() => "", "remote");

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Dispose_WhenCalled_ShouldDisposeTunnel()
        {
            var networkTunnel = Mock.Create<INetworkTunnel>();
            var networkConnector = new NetworkConnectorMock(networkTunnel);
            Mock.Arrange(() => _networkAddressFactory.Create(null)).IgnoreArguments()
                .Returns(Substitute.For<INetworkAddress>());
            _recorder = Mock.Create<IRecorder>();
            _sut = new TransmitSession(networkConnector, _networkAddressFactory, _recorder);
            _sut.Authorize(() => "", "remote");

            _sut.Dispose();;

            Mock.Assert(() => networkTunnel.Dispose(), Occurs.Once());
        }

        private class NetworkConnectorMock : INetworkConnector
        {
            private readonly INetworkTunnel _networkTunnel;
            
            public NetworkConnectorMock(INetworkTunnel networkTunnel)
            {
                _networkTunnel = networkTunnel;
            }

            public bool TryConnect(INetworkAddress networkAddress, out INetworkTunnel networkTunnel)
            {
                networkTunnel = _networkTunnel;
                return true;
            }
        }
    }
}