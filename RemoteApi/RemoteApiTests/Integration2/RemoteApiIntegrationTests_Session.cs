using System.Threading.Tasks;
using RemoteApi.Integration2.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration2
{
    public class RemoteApiIntegrationTestsSession
    {
        private ITestOutputHelper _output;
        
        public RemoteApiIntegrationTestsSession(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async void Operator_WhenConnectToRemoteSocket_ShouldDisconnectLocal()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22222");
            await apiOperator.Initialized;

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");
            
            await remote.Connected;

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, apiOperator.Sockets[1].CloseCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenRemoteSocketDisconnected_ShouldConnectAgain()
        {
            var wrongCommandCalledTimes = 0;
            var wrongCommandCalledTimesIntermediateState = 0;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22222");
            remote.ApiMap.RegisterWrongCommandHandler(() => wrongCommandCalledTimes++);
            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");
            await apiOperator.MessageDisplayed;
            
            remote.Sockets[1].Close();
            wrongCommandCalledTimesIntermediateState = wrongCommandCalledTimes;
            await apiOperator.Sockets[2].Disposed;

            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            await apiOperator.RaiseCommandReceived("hello");
            //await apiOperator.MessageDisplayed;

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
            await apiOperator.Initialized;
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");
            await remote.Connected;
            remote.Sockets[1].Close();
            remote.Sockets[0].Close();
            await apiOperator.Sockets[2].Disposed;

            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();
            
            apiOperator.RaiseCommandReceived("hello");
            await Task.Delay(1000);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, apiOperator.Sockets[1].CloseCalledTimes);
            Assert.Equal(1, apiOperator.Sockets[1].DisposeCalledTimes);
            Assert.Equal(2, apiOperator.Recorder.SystemErrorCalledTimes);
        }
    }
}