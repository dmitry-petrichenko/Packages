using System;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    [Collection("Serial")]
    public class ConnectParserTests
    {
        private IConnectParser _sut;
        private IRemoteApiOperator _remoteApiOperator;
        private IRecorder _recorder;
        private IApplicationRecorder _applicationRecorder;
        
        public ConnectParserTests()
        {
            _remoteApiOperator = Mock.Create<IRemoteApiOperator>();
            _recorder = Mock.Create<IRecorder>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            
            _sut = new ConnectParser(_remoteApiOperator, _applicationRecorder, _recorder);
        }
        
        [Fact]
        public void ExecuteConnect_WhenConnectSuccess_ShouldRaiseConnect()
        {
            var connectedParam = default(string);
            Mock.Arrange(() => _remoteApiOperator.Connect(Arg.AnyString)).Returns(Task.FromResult(true));
            _sut.Connected += s => connectedParam = s;
            
            _sut.ExecuteCommand("connect 127.0.0.1:11101");

            Assert.Equal("127.0.0.1:11101", connectedParam);
        }
        
        [Fact]
        public async void ExecuteConnect_WhenConnectFail_ShouldNotRaiseConnect()
        {
            var connectedParam = string.Empty;
            Mock.Arrange(() => _remoteApiOperator.Connect(Arg.AnyString)).Returns(Task.FromResult(false));
            _sut.Connected += s => connectedParam = s;
            
            var result = await _sut.ExecuteCommand("connect 127.0.0.1:11101");

            Assert.Equal(string.Empty, connectedParam);
            Assert.False(result);
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalledForConnect_ShouldConnect()
        {
            _sut.ExecuteCommand("connect 127.0.0.1:10101");

            Mock.Assert(() => _remoteApiOperator.Connect("127.0.0.1:10101"), Occurs.Once());
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalledForConnectWithWrongParameters_ShouldRecordAppInfo()
        {
            _sut.ExecuteCommand("connect");

            Mock.Assert(() => _applicationRecorder.RecordInfo(Arg.IsAny<string>(), Arg.IsAny<string>()), Occurs.Once());
        }
        
        [Fact]
        public void ExecuteCommand_WhenCalledForDisconnect_ShouldDisconnect()
        {
            _sut.ExecuteCommand("disconnect");

            Mock.Assert(() => _remoteApiOperator.Disconnect(), Occurs.Once());
        }
        
        [Fact]
        public void ExecuteCommand_NotConnectNorDisconnect_ShouldCallExecuteCommand()
        {
            _sut.ExecuteCommand("connect2");

            Mock.Assert(() => _remoteApiOperator.ExecuteCommand("connect2"), Occurs.Once());
        }
        
        [Fact]
        public void InstructionReceived_WhenRaised_ShouldRaiseInstructionReceived()
        {
            var received = string.Empty;
            _sut.InstructionReceived += s => received = s;
            
            Mock.Raise(() => _remoteApiOperator.InstructionReceived += null, "capacity");
            
            Assert.Equal("capacity", received);
        }
    }
}