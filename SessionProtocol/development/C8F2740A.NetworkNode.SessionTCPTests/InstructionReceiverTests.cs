using System.Collections.Generic;
using System.Linq;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class InstructionReceiverTests
    {
        private IInstructionReceiver _sut;
        private INodeGateway _nodeGateway;
        private ISessionHolder _sessionHolder;
        private IRecorder _recorder;

        public InstructionReceiverTests()
        {
            _nodeGateway = Mock.Create<INodeGateway>();
            _sessionHolder = Mock.Create<ISessionHolder>();
            _recorder = Mock.Create<IRecorder>();
            
            _sut = new InstructionReceiver(_nodeGateway, _sessionHolder, _recorder);
        }
        
        #region Constructor
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            _nodeGateway = Mock.Create<INodeGateway>();
            _sessionHolder = Mock.Create<ISessionHolder>();
            _recorder = Mock.Create<IRecorder>();
            _nodeGateway.ArrangeSet(x => x.ConnectionReceived += null).IgnoreArguments().Occurs(1);
            _sessionHolder.ArrangeSet(x => x.InstructionReceived += null).IgnoreArguments().Occurs(1);
            
            _sut = new InstructionReceiver(_nodeGateway, _sessionHolder, _recorder);
            
            _sessionHolder.AssertAll();
            _nodeGateway.AssertAll();
        }
        #endregion
        
        #region ConnectionReceived
        [Fact]
        public void ConnectionReceived_WhenCalledWith_ShouldSet()
        {
            var session = Mock.Create<ISession>();

            Mock.Raise(() =>        
                _nodeGateway.ConnectionReceived += null, session); 
            
            Mock.Assert(() => _sessionHolder.Set(session), Occurs.Exactly(1));
        }
        
        [Fact]
        public void ConnectionReceived_WhenCalledWithActiveSession_ShouldClear()
        {
            var session = Mock.Create<ISession>();
            Mock.Raise(() =>        
                _nodeGateway.ConnectionReceived += null, session);
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(true);
            Mock.Raise(() =>        
                _nodeGateway.ConnectionReceived += null, session); 
            
            Mock.Assert(() => _sessionHolder.Set(session), Occurs.Exactly(2));
            Mock.Assert(() => _sessionHolder.Clear(), Occurs.Exactly(1));
        }
        #endregion
        
        #region TrySendInstruction
        [Theory]
        [InlineData( 0b0100_1111 )]
        [InlineData( 0b0100_1001 )]
        [InlineData( 0b0100_1101 )]
        public void TrySendInstruction_WhenCalled_ShouldSendToSessionHolder(byte data)
        {
            var dataToSend = data.ToEnumerable();
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(true);
            _sut.TrySendInstruction(dataToSend);
            
            Mock.Assert(() => _sessionHolder.SendInstruction(dataToSend), Occurs.Exactly(1));
        }
        
        [Fact]
        public void TrySendInstruction_WhenCalledWithoutSession_ShouldSendCachError()
        {
            var dataToSend = ((byte)0b1011_1111).ToEnumerable();
            _sut.TrySendInstruction(dataToSend);
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Exactly(1));
        }
        #endregion
        
        #region InstructionReceived
        [Theory]
        [InlineData( 0b0100_1111 )]
        [InlineData( 0b0100_1001 )]
        [InlineData( 0b0100_1101 )]
        public void InstructionReceived_WhenRaised_RaiseUp(byte data)
        {
            var actual = default(IEnumerable<byte>);
            _sut.InstructionReceived += d =>
            {
                actual = d;
                return Enumerable.Empty<byte>();
            };

            Mock.Raise(() =>        
                _sessionHolder.InstructionReceived += null, data.ToEnumerable()); 
            
            Assert.Equal(data.ToEnumerable(), actual);
        }
        #endregion
        
        #region Dispose
        [Fact]
        public void Dispose_WhenCalled_ShouldClearAndUnsubscribe()
        {
            _nodeGateway.ArrangeSet(x => x.ConnectionReceived -= null).IgnoreArguments().Occurs(1);
            _sessionHolder.ArrangeSet(x => x.InstructionReceived -= null).IgnoreArguments().Occurs(1);
            
            _sut.Dispose();

            _sessionHolder.AssertAll();
            _nodeGateway.AssertAll();
            Mock.Assert(() => _sessionHolder.Clear(), Occurs.Exactly(1));
        }
        #endregion
    }
}