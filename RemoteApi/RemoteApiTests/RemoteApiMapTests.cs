using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C8F2740A.NetworkNode.SessionProtocol;
using RemoteApi;
using Xunit;

namespace SomeTests
{
    public class RemoteApiMapTests
    {
        private IRemoteApiMap _sut;
        private InstructionsReceiverMock _instructionsReceiverMock;
        
        public RemoteApiMapTests()
        {
            _instructionsReceiverMock = new InstructionsReceiverMock();
            _sut = new RemoteApiMap(_instructionsReceiverMock);
            Assert.True(true);
        }

        [Fact]
        public void RegisterCommand_WhenCalled_ShouldShowItOnCapacity()
        {
            _sut.RegisterCommand("add", () => null, "description");
            
            var result = _instructionsReceiverMock.SimulateCommandReceived(Encoding.UTF8.GetBytes("capacity"));
            var str = Encoding.UTF8.GetString(result.ToArray());
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
    }

    internal class InstructionsReceiverMock : IInstructionsReceiver
    {
        public event Func<IEnumerable<byte>, IEnumerable<byte>> CommandReceived;

        public IEnumerable<byte> SimulateCommandReceived(IEnumerable<byte> value)
        {
            return CommandReceived?.Invoke(value);
        }
    }
}