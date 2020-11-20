using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.NetworkNode.SessionProtocol.Tests
{
    public class NodeGatewayTests
    {
        private INodeGateway _sut;
        private INetworkPoint _networkPoint;
        private IRecorder _recorder;
        
        public NodeGatewayTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _networkPoint = Mock.Create<INetworkPoint>();
            _sut = new NodeGateway(_networkPoint, ReceiveSessionFactory, _recorder);
        }

        private IReceiveSession ReceiveSessionFactory(INetworkTunnel _)
        {
            return Mock.Create<IReceiveSession>();
        }

        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribeOnAcceptedConnection()
        {
            Mock.ArrangeSet(() => _networkPoint.Accepted += null).IgnoreArguments().OccursOnce();
            _sut = new NodeGateway(_networkPoint, ReceiveSessionFactory, _recorder);

            Mock.AssertAll(_networkPoint);
        }
        
        [Fact]
        public void ConnectionAcceptedHandler_OnAccepted_ShouldCreateSession()
        {
            var factoryWasCalled = false;
            var tunnelWasCalled = default(INetworkTunnel);
            var tunnel = Mock.Create<INetworkTunnel>();

            _sut = new NodeGateway(_networkPoint, ReceiveSessionFactory, _recorder);
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);

            IReceiveSession ReceiveSessionFactory(INetworkTunnel networkTunnel)
            {
                tunnelWasCalled = networkTunnel;
                factoryWasCalled = true;
                return Mock.Create<IReceiveSession>();
            }

            Assert.Same(tunnel, tunnelWasCalled);
            Assert.True(factoryWasCalled);
        }
        
        [Fact]
        public void ConnectionAcceptedHandler_OnValidateFalse_ShouldDisposeSession()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(false));
            
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            Mock.Assert(() => session.Dispose(), Occurs.Once());
        }
        
        [Fact]
        public void ConnectionAcceptedHandler_OnValidate_ShouldSubscribeOnSession()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            Mock.ArrangeSet(() => session.Received += null).IgnoreArguments().OccursOnce();
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            Mock.AssertAll(session);
        }
        
        [Fact]
        public void ConnectionAcceptedHandler_OnValidate_ShouldSubscribeOnClose()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            Mock.ArrangeSet(() => session.Closed += null).IgnoreArguments().OccursOnce();
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            Mock.AssertAll(session);
        }
        
        [Fact]
        public void SendCommand_OnValidate_ShouldSendCommandInSession()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            _sut.SendCommand(new byte[] { 0b0011_0011 });
            
            Mock.Assert(() => session.SendCommand(new byte[] { 0b0011_0011 }), Occurs.Once());
        }
        
        [Fact]
        public void Validate_WhenValidationInProcess_ShouldUnsubscribeFromAccepted()
        {
            var networkPoint = Mock.Create<INetworkPoint>();
            var validateResult = new TaskCompletionSource<bool>();
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(validateResult.Task);
            _sut = new NodeGateway(networkPoint, _ => session, _recorder);
            Mock.ArrangeSet(() => networkPoint.Accepted -= null).IgnoreArguments().OccursOnce();
            Mock.ArrangeSet(() => networkPoint.Accepted += null).IgnoreArguments().OccursOnce();
            
            Mock.Raise(() => networkPoint.Accepted += null, tunnel);
            
            Mock.AssertAll(networkPoint);
        }
        
        [Fact]
        public void Validate_WhenValidationSuccess_ShouldSubscribeOnAccepted()
        {
            var networkPoint = Mock.Create<INetworkPoint>();
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            _sut = new NodeGateway(networkPoint, _ => session, _recorder);
            Mock.ArrangeSet(() => networkPoint.Accepted += null).IgnoreArguments().Occurs(2);
            Mock.ArrangeSet(() => networkPoint.Accepted -= null).IgnoreArguments().OccursOnce();
            
            Mock.Raise(() => networkPoint.Accepted += null, tunnel);
            
            Mock.AssertAll(networkPoint);
        }
        
        [Fact]
        public void Dispose_OnCall_ShouldUnsubscribeFromSession()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            Mock.ArrangeSet(() => session.Received -= null).IgnoreArguments().OccursOnce();
            Mock.ArrangeSet(() => session.Closed -= null).IgnoreArguments().OccursOnce();
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            _sut.Dispose();

            Mock.AssertAll(session);
        }
        
        [Fact]
        public void Dispose_OnCall_ShouldCallSeccionDispose()
        {
            var tunnel = Mock.Create<INetworkTunnel>();
            var session = Mock.Create<IReceiveSession>();
            Mock.Arrange(() => session.Validate()).Returns(Task.FromResult(true));
            _sut = new NodeGateway(_networkPoint, _ => session, _recorder);
            Mock.Raise(() => _networkPoint.Accepted += null, tunnel);
            
            _sut.Dispose();

            Mock.Assert(()=> session.Dispose(), Occurs.Once());
        }
    }
}
