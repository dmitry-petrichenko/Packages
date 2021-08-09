using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionTCP;
using C8F2740A.NetworkNode.SessionTCP.Impl;
using Telerik.JustMock;
using Telerik.JustMock.AutoMock.Ninject.Infrastructure.Language;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class InstructionSenderTests
    {
        private IInstructionSender _sut;
        private INetworkAddress _networkAddress;
        private INodeVisitor _nodeVisitor;
        private ISessionHolder _sessionHolder;
        private IRecorder _recorder;

        public InstructionSenderTests()
        {
            _networkAddress = Mock.Create<INetworkAddress>();
            _nodeVisitor = Mock.Create<INodeVisitor>();
            _sessionHolder = Mock.Create<ISessionHolder>();
            _recorder = Mock.Create<IRecorder>();
            
            _sut = new InstructionSender(_nodeVisitor, _networkAddress, _sessionHolder, _recorder);
        }

        #region Dispose
        [Fact]
        public void Dispose_WhenCalled_ShouldClear()
        {
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(true);

            _sut.Dispose();
            
            Mock.Assert(() => _sessionHolder.Clear(), Occurs.Exactly(1));
        }
        #endregion
        
        #region TrySendInstruction
        [Theory]
        [InlineData( new byte[] { 0b0101_1111, 0b0101_1101} )]
        [InlineData( new byte[] { 0b1101_1101, 0b1101_0101} )]
        public void TrySendInstruction_WhenCalledWithActiveSession_ShouldSend(IEnumerable<byte> data)
        {
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(true);
            
            _sut.TrySendInstruction(data);
            
            Mock.Assert(() => _sessionHolder.SendInstruction(data), Occurs.Exactly(1));
        }
        
        [Theory]
        [InlineData( new byte[] { 0b1101_1111, 0b0101_1101} )]
        [InlineData( new byte[] { 0b1111_1111, 0b1101_0101} )]
        public void TrySendInstruction_WhenCalledWithNoActiveSession_ShouldRecordError(IEnumerable<byte> data)
        {
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(false);

            _sut.TrySendInstruction(data);
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Exactly(1));
        }
        
        [Theory]
        [InlineData( new byte[] { 0b1101_1111, 0b0101_1101} )]
        [InlineData( new byte[] { 0b1111_1111, 0b1101_0101} )]
        public void TrySendInstruction_WhenCalledWithNoActiveSessionOnNoConnection_ShouldReturnFalse(IEnumerable<byte> data)
        {
            var session = Mock.Create<ISession>();
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(false);

            _sut.TrySendInstruction(data);
            
            Mock.Assert(() => _sessionHolder.SendInstruction(data), Occurs.Exactly(0));
            Mock.Assert(() => _sessionHolder.Set(session), Occurs.Exactly(0));
        }
        
        [Fact]
        public void TrySendInstruction_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _sessionHolder.HasActiveSession).Returns(true);
            Mock.Arrange(() => _sessionHolder.SendInstruction(Arg.IsAny<IEnumerable<byte>>())).Throws<Exception>();

            _sut.TrySendInstruction(((byte)0b1101_1111).ToEnumerable());
            
            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        #endregion
    }
}