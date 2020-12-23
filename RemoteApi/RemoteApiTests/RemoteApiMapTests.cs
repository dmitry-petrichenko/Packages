using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace RemoteApi
{
    public class RemoteApiMapTests
    {
        private IRemoteApiMap _sut;
        private InstructionsReceiverMock _instructionsReceiverMock;
        
        public RemoteApiMapTests()
        {
            _instructionsReceiverMock = new InstructionsReceiverMock();
            _sut = new RemoteApiMap(_instructionsReceiverMock);
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            var instructionReceiver = Mock.Create<IInstructionReceiver>();
            instructionReceiver.ArrangeSet(x => x.InstructionReceived += null).IgnoreArguments().Occurs(1);
            _sut = new RemoteApiMap(instructionReceiver);

            instructionReceiver.AssertAll();
        }

        [Fact]
        public void RegisterCommand_WhenCalled_ShouldShowItOnCapacity()
        {
            _sut.RegisterCommand("add", () => null, "description");
            
            var result = _instructionsReceiverMock.SimulateCommandReceived("capacity".ToEnumerableByte());
            var str = result.ToText();
            var lines = str.Split(Environment.NewLine);
            
            Assert.Equal(3, lines.Length);
        }
        
        [Fact]
        public void RegisterCommandWithParameters_WhenCommandRaised_ShouldCallHandlerWithParameters()
        {
            IEnumerable<string> acceptedParameters = Enumerable.Empty<string>();
            
            IEnumerable<byte> ResetHandler(IEnumerable<string> parameters)
            {
                acceptedParameters = parameters;
                return null;
            }
            
            _sut.RegisterCommandWithParameters("reset", ResetHandler);
            
            var result = _instructionsReceiverMock.SimulateCommandReceived(Encoding.UTF8.GetBytes("reset p:12 s:21"));

            Assert.Equal("p:12", acceptedParameters.First());
            Assert.Equal("s:21", acceptedParameters.ElementAtOrDefault(1));
        }
        
        [Fact]
        public void Received_WhenWrongCommand_ShouldReturnWrongCommand()
        {
            var result = _instructionsReceiverMock.SimulateCommandReceived("reset p:12".ToEnumerableByte());

            Assert.Equal(RemoteApiCommands.WRONG_COMMAND.ToEnumerableByte(), result);
        }
    }

    internal class InstructionsReceiverMock : IInstructionReceiver
    {
        public IEnumerable<byte> SimulateCommandReceived(IEnumerable<byte> value)
        {
            return InstructionReceived?.Invoke(value);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<(bool, IEnumerable<byte>)> TrySendInstruction(IEnumerable<byte> instruction)
        {
            throw new NotImplementedException();
        }

        public event Func<IEnumerable<byte>, IEnumerable<byte>> InstructionReceived;
    }
}