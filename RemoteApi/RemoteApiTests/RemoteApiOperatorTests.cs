using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace RemoteApi
{
    public class RemoteApiOperatorTests
    {
        private IRemoteApiOperator _sut;
        private IInstructionSenderHolder _instructionSenderHolder;
        private IInstructionSenderFactory _instructionSenderFactory;
        private IRecorder _recorder;
        
        public RemoteApiOperatorTests()
        {
            _instructionSenderHolder = Mock.Create<IInstructionSenderHolder>();
            _instructionSenderFactory = Mock.Create<IInstructionSenderFactory>();
            _recorder = Mock.Create<IRecorder>();
            
            _sut = new RemoteApiOperator(_instructionSenderHolder, _instructionSenderFactory, _recorder);
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            var instructionSenderHolder = Mock.Create<IInstructionSenderHolder>();
            instructionSenderHolder.ArrangeSet(x => x.InstructionReceived += null).IgnoreArguments().Occurs(1);

            _sut = new RemoteApiOperator(instructionSenderHolder, _instructionSenderFactory, _recorder);

            instructionSenderHolder.AssertAll();
        }
        
        [Fact]
        public void InstructionReceived_WhenCalled_ShouldRaiseInstructionReceived()
        {
            bool isReceived = false;
            _sut.InstructionReceived += s => isReceived = true;
            
            Mock.Raise(() => _instructionSenderHolder.InstructionReceived += null, "capacity".ToEnumerableByte());

            Assert.True(isReceived);
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalled_ShouldTrySendInstruction()
        {
            Mock.Arrange(() => _instructionSenderHolder.HasActiveSender).Returns(true);
            Mock.Arrange(() => _instructionSenderHolder.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()))
                .Returns(Task.FromResult((true, Enumerable.Empty<byte>())));
            _sut.ExecuteCommand("command");

            Mock.Assert(() => _instructionSenderHolder.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()), Occurs.Once());
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Never());
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalledWithoutSender_ShouldNotSendInstruction()
        {
            Mock.Arrange(() => _instructionSenderHolder.HasActiveSender).Returns(false);
            Mock.Arrange(() => _instructionSenderHolder.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()))
                .Returns(Task.FromResult((true, Enumerable.Empty<byte>())));
            _sut.ExecuteCommand("command");

            Mock.Assert(() => _instructionSenderHolder.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()), Occurs.Never());
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Connect_WhenCalled_ShouldCheckCorrectIP()
        {
            _sut.Connect("123.3333");
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Connect_WhenCalledWithCorrectAddress_ShouldTrySendInstruction()
        {
            var instructionSender = Mock.Create<IInstructionSender>();
            Mock.Arrange(() => _instructionSenderFactory.Create(Arg.IsAny<string>())).Returns(instructionSender);

            _sut.Connect("127.0.0.1:10101");
            
            Mock.Assert(() => instructionSender.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()), Occurs.Once());
        }
        
        [Fact]
        public void TrySendInstruction_WhenReturnsTrue_ShouldSetHolder()
        {
            var instructionSender = Mock.Create<IInstructionSender>();
            Mock.Arrange(() => instructionSender.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()))
                .Returns(Task.FromResult((true, Enumerable.Empty<byte>())));
            Mock.Arrange(() => _instructionSenderFactory.Create(Arg.IsAny<string>())).Returns(instructionSender);

            _sut.Connect("127.0.0.1:10101");
            
            Mock.Assert(() => _instructionSenderHolder.Set(Arg.IsAny<IInstructionSender>()), Occurs.Once());
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Never());
        }
        
        [Fact]
        public void TrySendInstruction_WhenReturnsFalse_ShouldNotSetHolder()
        {
            var instructionSender = Mock.Create<IInstructionSender>();
            Mock.Arrange(() => instructionSender.TrySendInstruction(Arg.IsAny<IEnumerable<byte>>()))
                .Returns(Task.FromResult((false, Enumerable.Empty<byte>())));
            Mock.Arrange(() => _instructionSenderFactory.Create(Arg.IsAny<string>())).Returns(instructionSender);

            _sut.Connect("127.0.0.1:10101");
            
            Mock.Assert(() => _instructionSenderHolder.Set(Arg.IsAny<IInstructionSender>()), Occurs.Never());
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void Disconnect_WhenCalled_ShouldClearSenderHolder()
        {
            Mock.Arrange(() => _instructionSenderHolder.HasActiveSender).Returns(true);

            _sut.Disconnect();
            
            Mock.Assert(() => _instructionSenderHolder.Clear(), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Disconnect_WhenCalledWithNoActiveSender_ShouldNotClearSenderHolder()
        {
            Mock.Arrange(() => _instructionSenderHolder.HasActiveSender).Returns(false);

            _sut.Disconnect();
            
            Mock.Assert(() => _instructionSenderHolder.Clear(), Occurs.Exactly(0));
        }
    }
}