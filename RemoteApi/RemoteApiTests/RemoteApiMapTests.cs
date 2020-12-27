using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
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
        private IRecorder _recorder;
        
        public RemoteApiMapTests()
        {
            _instructionsReceiverMock = new InstructionsReceiverMock();
            _recorder = Mock.Create<IRecorder>();
            _sut = new RemoteApiMap(_instructionsReceiverMock, _recorder);
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            var instructionReceiver = Mock.Create<IInstructionReceiver>();
            instructionReceiver.ArrangeSet(x => x.InstructionReceived += null).IgnoreArguments().Occurs(1);
            _sut = new RemoteApiMap(instructionReceiver, _recorder);

            instructionReceiver.AssertAll();
        }

        [Fact]
        public void RegisterCommand_WhenCalled_ShouldShowItOnCapacity()
        {
        }
        
        [Fact]
        public void RegisterCommandWithParameters_WhenCommandRaised_ShouldCallHandlerWithParameters()
        {
            IEnumerable<string> acceptedParameters = Enumerable.Empty<string>();
            
            void ResetHandler(IEnumerable<string> parameters)
            {
                acceptedParameters = parameters;
            }
            
            _sut.RegisterCommandWithParameters("reset", ResetHandler);
            
            var result = _instructionsReceiverMock.SimulateCommandReceived(Encoding.UTF8.GetBytes("reset p:12 s:21"));

            Assert.Equal("p:12", acceptedParameters.First());
            Assert.Equal("s:21", acceptedParameters.ElementAtOrDefault(1));
        }
        
        [Fact]
        public void Received_WhenWrongCommand_ShouldCallWrongCommandHandler()
        {
            bool wasCalled = false;
            _sut.RegisterWrongCommandHandler(() => wasCalled = true);
            var result = _instructionsReceiverMock.SimulateCommandReceived("reset p:12".ToEnumerableByte());
            
            Assert.True(wasCalled);
            Assert.Equal(result, Enumerable.Empty<byte>());
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