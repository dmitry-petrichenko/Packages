using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionProtocol;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class SessionTests
    {
        private Session _sut;
        private INetworkTunnel _networkTunnel;
        private IRecorder _recorder;
        
        public SessionTests()
        {
            _networkTunnel = Mock.Create<INetworkTunnel>();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldStartListen()
        {
            _sut = new Session(_networkTunnel, _recorder);
            
            Mock.Assert(() => _networkTunnel.Listen(), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribeOnClose()
        {
            _networkTunnel.ArrangeSet(x => x.Closed += null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _networkTunnel.AssertAll();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribeOnReceived()
        {
            _networkTunnel.ArrangeSet(x => x.Received += null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _networkTunnel.AssertAll();
        }
        
        [Fact]
        public void Dispose_WhenCalled_ShouldSubscribeFromSubscriptions()
        {
            _networkTunnel.ArrangeSet(x => x.Received -= null).IgnoreArguments().Occurs(1);
            _networkTunnel.ArrangeSet(x => x.Closed -= null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _sut.Dispose();;
            
            _networkTunnel.AssertAll();
        }
    }
}