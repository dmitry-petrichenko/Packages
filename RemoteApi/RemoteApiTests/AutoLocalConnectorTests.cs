using System.Threading.Tasks;
using C8F2740A.Common.Records;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    public class AutoLocalConnectorTests
    {
        private IAutoLocalConnector _sut;
        private IConnectParser _connectParser;
        private IRecorder _recorder;
        
        public AutoLocalConnectorTests()
        {
            _connectParser = Mock.Create<IConnectParser>();
            _recorder = Mock.Create<IRecorder>();
            
            _sut = new AutoLocalConnector(_connectParser, _recorder, "127.0.0.1:10000");
        }
        
        [Fact]
        public void InstructionReceived_WhenRaised_ShouldRaiseTextReceived()
        {
            var received = string.Empty;
            _sut.TextReceived += s => received = s;
            
            Mock.Raise(() => _connectParser.InstructionReceived += null, "capacity");
            
            Assert.Equal("capacity", received);
        }
        
        [Fact]
        public void InstructionReceived_WhenRaisedWithEmpty_ShouldRecordError()
        {
            var received = "string.Empty";
            _sut.TextReceived += s => received = s;
            
            Mock.Raise(() => _connectParser.InstructionReceived += null, "");
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
            Assert.Equal("string.Empty", received);
        }
        
        [Fact]
        public void Start_WhenCalled_ShouldExecuteCommandConnect()
        {
            _sut.Start();

            Mock.Assert(() => _connectParser.ExecuteCommand("connect 127.0.0.1:10000"), Occurs.Once());
        }

        [Fact]
        public async void Start_WhenCalledConnectFalse_ShouldReturnFalse()
        {
            Mock.Arrange(() => _connectParser.ExecuteCommand(Arg.AnyString))
                .IgnoreArguments()
                .Returns(Task.FromResult<bool>(false));
                
            _sut.Start();
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalled_ShouldExecuteCommand()
        {
            _sut.ExecuteCommand("command");

            Mock.Assert(() => _connectParser.ExecuteCommand("command"), Occurs.Once());
        }
        
        [Fact]
        public void Connected_WhenConnectedRaise_ShouldRaise()
        {
            var raised = string.Empty;
            _sut.Connected += s => raised = s;
            
            Mock.Raise(() => _connectParser.Connected += null, "127.0.0.1");
            
            Assert.Equal("127.0.0.1", raised);
        }
    }
}