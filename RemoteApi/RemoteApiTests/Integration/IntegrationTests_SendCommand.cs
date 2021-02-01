using RemoteApi.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    public class IntegrationTests_SendCommand
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_SendCommand(ITestOutputHelper output)
        {
            _output = output;
        }
        
        public void Dispose()
        {
        }

        [Theory]
        [InlineData(1, 1, 1, "11111", "22222")]
        [InlineData(2, 2, 2, "33333", "44444")]
        [InlineData(3, 3, 3, "55555", "55556")]
        public async void Operator_WhenWrongCommandSend_ShouldDisplayInMonitor(
            int expectedMessageDisplays, 
            int sendWrongCommandTimes, 
            int expectedWrongCommandCalled, 
            string localPort,
            string remotePort)
        {
            var actualWrongCommandCalledTimes = 0;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets($"127.0.0.1:{localPort}");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets($"127.0.0.1:{remotePort}");
            remote.ApiMap.RegisterWrongCommandHandler(() =>
            {
                actualWrongCommandCalledTimes++;
                ((IApplicationRecorder) remote.Recorder).RecordInfo("wrong", "wrong");
            });

            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived($"connect 127.0.0.1:{remotePort}");
            await remote.ConnectedComplete;
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            for (int i = 0; i < sendWrongCommandTimes; i++)
            {
                await apiOperator.RaiseCommandReceived("hello");
                await apiOperator.MessageDisplayed;
            }

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(expectedWrongCommandCalled, actualWrongCommandCalledTimes);
            Assert.Equal(expectedMessageDisplays, apiOperator.Recorder.DisplayMessagesCalledTimes);
        }
        
        [Theory]
        [InlineData(1, 1, 1, "8111", "8222")]
        [InlineData(2, 2, 2, "8333", "8444")]
        [InlineData(3, 3, 3, "8555", "8666")]
        public async void Operator_WhenCommandSend_ShouldDisplayInMonitor2(
            int expectedMessageDisplays, 
            int sendCommandTimes, 
            int expectedWrongCommandCalled, 
            string localPort,
            string remotePort)
        {
            var actualCommandCalledTimes = 0;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets($"127.0.0.1:{localPort}");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets($"127.0.0.1:{remotePort}");
            remote.ApiMap.RegisterCommand("someCommand", () =>
            {
                actualCommandCalledTimes++;
                ((IApplicationRecorder) remote.Recorder).RecordInfo("", "some command executed");
            });

            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived($"connect 127.0.0.1:{remotePort}");
            await remote.ConnectedComplete;
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            for (int i = 0; i < sendCommandTimes; i++)
            {
                await apiOperator.RaiseCommandReceived("someCommand");
                await apiOperator.MessageDisplayed;
            }

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(expectedWrongCommandCalled, actualCommandCalledTimes);
            Assert.Equal(expectedMessageDisplays, apiOperator.Recorder.DisplayMessagesCalledTimes);
        }
        
        [Theory]
        [InlineData(1, 1, "a", "11118", "22229")]
        [InlineData(1, 3, "abc", "11121", "22231")]
        [InlineData(2, 8, "abce", "11122", "22232")]
        public async void Operator_WhenCommandWithParametersSend_ShouldGetParameters(
            int sendCommandTimes, 
            int allReceivedParamsCountExpected, 
            string sentParameter, 
            string localPort,
            string remotePort)
        {
            var allReceivedParamsCountActual = 0;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets($"127.0.0.1:{localPort}");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets($"127.0.0.1:{remotePort}");
            remote.ApiMap.RegisterCommandWithParameters("commandWithParam", parameter =>
            {
                foreach (var one in parameter)
                {
                    allReceivedParamsCountActual += one.Length;
                }
                ((IApplicationRecorder) remote.Recorder).RecordInfo("", "executed");
            });

            await apiOperator.MessageDisplayed;
            await apiOperator.RaiseCommandReceived($"connect 127.0.0.1:{remotePort}");
            await remote.ConnectedComplete;
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            for (int i = 0; i < sendCommandTimes; i++)
            {
                await apiOperator.RaiseCommandReceived($"commandWithParam {sentParameter}");
                await apiOperator.MessageDisplayed;
            }

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(allReceivedParamsCountActual, allReceivedParamsCountExpected);
        }
    }
}