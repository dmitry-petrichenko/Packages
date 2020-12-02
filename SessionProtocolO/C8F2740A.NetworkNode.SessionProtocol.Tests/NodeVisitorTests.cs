using System;
using C8F2740A.Common.Records;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.NetworkNode.SessionProtocol.Tests
{
    public class NodeVisitorTests
    {
        private INodeVisitor _sut;
        private ITransmitSessionFactory _transmitSessionFactory;
        private IRecorder _recorder;

        public NodeVisitorTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _transmitSessionFactory = Mock.Create<ITransmitSessionFactory>();
            _sut = new NodeVisitor("", _transmitSessionFactory, _recorder);
        }
        
        [Fact]
        public void TrySendCommand_OnFirstCall_ShouldCreateSession()
        {
            // Act 
            _sut.TrySendCommand(Array.Empty<byte>(), "");
            
            //Assert 
            Mock.Assert(() => _transmitSessionFactory.Create(), Occurs.Once());
        }
        
        [Fact]
        public void TrySendCommand_OnFirstCall_ShouldSubscribeOnSession()
        {
            // Arrange
            var session = Mock.Create<ITransmitSession>();
            Mock.Arrange(() => _transmitSessionFactory.Create()).Returns(session);
            Mock.ArrangeSet(() => session.SessionClosed += null).IgnoreArguments().OccursOnce();
            
            // Act 
            _sut.TrySendCommand(Array.Empty<byte>(), "");
            
            //Assert 
            Mock.AssertAll(session);
        }
    }
}