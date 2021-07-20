using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi.Integration.Helpers;
using SocketSubstitutionTests;
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
        public async void Operator_ConnectAndDisconnect_ShouldWorkAsExpected()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2("127.0.0.1:11117");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11118");
            
            await IntegrationTestsHelpers.WaitingConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11118");
            await IntegrationTestsHelpers.WaitingConnectComplete(apiOperator, "connect_2");
            
            // Act
            await apiOperator.RaiseCommandReceived("disconnect");
            await IntegrationTestsHelpers.WaitingConnectComplete(apiOperator, "connect_3");
            
            //Assert
            Assert.Equal(0, apiOperator.Recorder.AppErrorCalledTimes);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            
            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_ConnectAndCommandAndDisconnectAndConnect_ShouldDisplayMessages()
        {
            // Arrange
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2("127.0.0.1:11119");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11120");
            remote.ApiMap.RegisterCommand("example", () =>
            {
                ((IApplicationRecorder) remote.Recorder).RecordInfo("cmd", "executed");
            });
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11120");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");
            
            var displayedTest = apiOperator.Recorder.ArrangeWaitingMessage("cmd:executed", 5000);
            await apiOperator.RaiseCommandReceived("example");

            // wait command complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 4);

            var res = await displayedTest;
            Assert.True(res);
            
            // Act
            await apiOperator.RaiseCommandReceived("disconnect");
            await IntegrationTestsHelpers.AssertCloseComplete(apiOperator, "connect_2");
            
            displayedTest = apiOperator.Recorder.ArrangeWaitingMessage("cmd:executed", 5000);
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11120");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_4");

            res = await displayedTest;
            Assert.True(res);
            
            Assert.Equal(0, apiOperator.Recorder.AppErrorCalledTimes);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);

            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }

        [Fact]
        public async void Operator_ConnectCommandDisconnectSeveralDifferent_ShouldShowMessagesCorrect()
        {
            // Arrange
            var execute1 = false;
            var execute2 = false;
            var execute3 = false;
            
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2("127.0.0.1:11121");
            
            var remote1 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11122");
            remote1.ApiMap.RegisterCommand("execute1", () =>
            {
                execute1 = true;
            });
            var remote2 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11123");
            remote2.ApiMap.RegisterCommand("execute2", () =>
            {
                execute2 = true;
            });
            var remote3 = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11124");
            remote3.ApiMap.RegisterCommand("execute3", () =>
            {
                execute3 = true;
            });
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11122");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");
            await apiOperator.RaiseCommandReceived("execute1");
            // wait command complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 4);
            //--
            await apiOperator.RaiseCommandReceived("disconnect");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_3");

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11123");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_4");
            await apiOperator.RaiseCommandReceived("execute2");
            // wait command complete
            var connect4 = apiOperator.GetSocketByTag("connect_4");
            await connect4.ArrangeWaiting(connect4.ReceiveCalledTimes, 4);
            //--
            await apiOperator.RaiseCommandReceived("disconnect");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_5");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11124");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_6");
            await apiOperator.RaiseCommandReceived("execute3");
            // wait command complete
            var connect6 = apiOperator.GetSocketByTag("connect_6");
            await connect6.ArrangeWaiting(connect6.ReceiveCalledTimes, 4);
            //--
            await apiOperator.RaiseCommandReceived("disconnect");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_7");
            
            Assert.True(execute1);
            Assert.True(execute2);
            Assert.True(execute3);
            Assert.Equal(0, apiOperator.Recorder.AppErrorCalledTimes);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);

            // Log
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
        }
    }
}