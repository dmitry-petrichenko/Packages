using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    public class IntegrationTests_Disconnect
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_Disconnect(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async void Operator_DisconnectCommand_ShouldDisconnect()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:9111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:9222");
            await apiOperator.MessageDisplayed;

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:9222");
            await apiOperator.MessageDisplayed;
            await remote.ConnectedComplete;
            apiOperator.Recorder.ClearCache();
            
            // Act
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            //Assert
            Assert.False(apiOperator.Sockets[2].Connected);
            Assert.Equal(1, apiOperator.Sockets[2].DisposeCalledTimes);
            Assert.True(apiOperator.Sockets[5].Connected);
            Assert.Equal(2, apiOperator.Sockets[5].ReceiveCalledTimes);
            Assert.False(remote.Sockets[1].Connected);
            
            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_CommandAndConnectAndDisconnect_ShouldDisplayMessages()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:9333");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:9444");
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("hi");
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:9444");
            await apiOperator.MessageDisplayed;
            await remote.ConnectedComplete;
            
            // Act
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            //Assert
            Assert.Equal(4, apiOperator.Recorder.DisplayMessagesCalledTimes);
            
            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
        }
        
        [Fact]
        public async void Operator_ConnectAndCommandAndDisconnectAndConnect_ShouldDisplayMessages()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:9555");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:9666");
            remote.ApiMap.RegisterCommand("example", () =>
            {
                ((IApplicationRecorder) remote.Recorder).RecordInfo("cmd", "executed");
            });

            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:9666");
            await apiOperator.MessageDisplayed;
            await remote.ConnectedComplete;
            await apiOperator.RaiseCommandReceived("example");
            await apiOperator.MessageDisplayed;
            
            // Act
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:9666");
            await apiOperator.MessageDisplayed;
            await remote.ConnectedComplete;
            
            //Assert
            Assert.Equal(5, apiOperator.Recorder.DisplayMessagesCalledTimes);
            Assert.Equal(1, remote.Recorder.AppInfoCalledTimes);

            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_ConnectCommandDisconnectSeveral_ShouldShowMessagesCorrect()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:9777");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:29888");
            await apiOperator.MessageDisplayed;
            remote.ApiMap.RegisterCommand("execute", () =>
            {
                ((IApplicationRecorder) remote.Recorder).RecordInfo("cmd", "executed");
            });

            for (int i = 0; i < 2; i++)
            {
                await apiOperator.RaiseCommandReceived("connect 127.0.0.1:29888");
                await apiOperator.MessageDisplayed;
                await remote.ConnectedComplete;
            
                await apiOperator.RaiseCommandReceived("execute");
                await apiOperator.MessageDisplayed;
            
                await apiOperator.RaiseCommandReceived("disconnect");
                await apiOperator.MessageDisplayed;
            }
            
            //Assert
            Assert.False(false);

            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_ConnectCommandDisconnectSeveralDifferent_ShouldShowMessagesCorrect()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:12121");
            var remote1 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:10101");
            remote1.ApiMap.RegisterCommand("execute1", () =>
            {
                ((IApplicationRecorder) remote1.Recorder).RecordInfo("cmd", "executed1");
            });
            var remote2 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:20202");
            remote2.ApiMap.RegisterCommand("execute2", () =>
            {
                ((IApplicationRecorder) remote2.Recorder).RecordInfo("cmd", "executed2");
            });
            var remote3 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:30303");
            remote3.ApiMap.RegisterCommand("execute3", () =>
            {
                ((IApplicationRecorder) remote3.Recorder).RecordInfo("cmd", "executed3");
            });
            
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:10101");
            await apiOperator.MessageDisplayed;
            await remote1.ConnectedComplete;
            await apiOperator.RaiseCommandReceived("execute1");
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:20202");
            await apiOperator.MessageDisplayed;
            await remote1.ConnectedComplete;
            await apiOperator.RaiseCommandReceived("execute2");
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:30303");
            await apiOperator.MessageDisplayed;
            await remote1.ConnectedComplete;
            await apiOperator.RaiseCommandReceived("execute3");
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("disconnect");
            await apiOperator.MessageDisplayed;
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:10101");
            await apiOperator.MessageDisplayed;
            await remote1.ConnectedComplete;

            //Assert
            Assert.False(false);

            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
        }
    }
}