using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi.Integration.Helpers;
using SocketSubstitutionTests;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    public class IntegrationTests_Connect
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_Connect(ITestOutputHelper output)
        {
            _output = output;
        }

        private Task<bool> WaitingInitializationComplete(RemoteOperatorTestWrapperRealSockets2 apiOperator)
        {
            var connect1 = apiOperator.GetSocketByTag("connect_1");
            return connect1.ArrangeWaiting(connect1.SendCalledTimes, 2);
        }
        
        [Fact]
        public async void Operator_WhenConnectToRemoteSocket_ShouldDisconnectLocal2()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2("127.0.0.1:11111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11112");
            
            var connect1 = apiOperator.GetSocketByTag("connect_1");
            var res = await connect1.ArrangeWaiting(connect1.SendCalledTimes, 2);
            Assert.True(res);

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11112");
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            var res2 = await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 2);
            Assert.True(res2);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            
            // Assert
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            Assert.Equal(1, connect1.DisposeCalledTimes.Value);
        }

        [Fact]
        public async void Operator_WhenRemoteSocketDisconnected_ShouldConnectToRemoteAgainToExecuteCommand()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2("127.0.0.1:11113");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2("127.0.0.1:11114");
            remote.ApiMap.RegisterWrongCommandHandler(() => { });
            
            await WaitingInitializationComplete(apiOperator);
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11114");

            // wait receive complete
            var accept1 = remote.GetSocketByTag("accept_1");
            await accept1.ArrangeWaiting(accept1.ReceiveCalledTimes, 2);
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();
            
            accept1.Close();
            
            // wait close complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.CloseCalledTimes, 1);

            await apiOperator.RaiseCommandReceived("hello");

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            
            // Assert
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
        }

        //-----------------------------------------

        [Fact]
        public async void Operator_WhenRemoteSocketDisconnected_ShouldConnectAgain()
        {
            var wrongCommandCalledTimes = 0;
            var wrongCommandCalledTimesIntermediateState = 0;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11112");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22223");
            remote.ApiMap.RegisterWrongCommandHandler(() => wrongCommandCalledTimes++);
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22223");
            await remote.ConnectedComplete;
            
            remote.Sockets[1].Close();
            wrongCommandCalledTimesIntermediateState = wrongCommandCalledTimes;
            await apiOperator.Sockets[2].Disposed;

            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            await apiOperator.RaiseCommandReceived("hello");

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, apiOperator.Sockets[1].CloseCalledTimes);
            Assert.Equal(1, wrongCommandCalledTimes);
            Assert.Equal(0, wrongCommandCalledTimesIntermediateState);
        }
        
        [Fact]
        public async void Operator_WhenRemoteSocketDisconnectedAndLost_ShouldTryConnectAndShowMessage()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22222");
            remote.ApiMap.RegisterWrongCommandHandler(() =>
                ((IApplicationRecorder) remote.Recorder).RecordInfo("wrong", "wrong"));
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");
            await remote.ConnectedComplete;
            
            remote.Sockets[1].Close();
            remote.Sockets[0].Close();
            await apiOperator.Sockets[2].Disposed;

            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();
            
            await apiOperator.RaiseCommandReceived("hello");

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, apiOperator.Sockets[1].CloseCalledTimes);
            Assert.Equal(1, apiOperator.Sockets[1].DisposeCalledTimes);
            Assert.Equal(2, apiOperator.Recorder.SystemErrorCalledTimes);
        }
    }
}