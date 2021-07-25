using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi.Integration.Helpers;
using SocketSubstitutionTests;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    [Collection("Serial")]
    public class IntegrationTests_SendCommand
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_SendCommand(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void Operator_WhenCommandSend_ShouldDisplayInMonitor2()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2($"127.0.0.1:22221");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2($"127.0.0.1:22222");
            remote.ApiMap.RegisterCommand("command1", () =>
            {
                ((IApplicationRecorder) remote.Recorder).RecordInfo("sme", "done");
            });
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");
            var operatorDisplayedTest = apiOperator.Recorder.ArrangeWaitingMessage("sme:done", 5000);
            await apiOperator.RaiseCommandReceived("command1");
            // wait command complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 4);
            //--

            var res1 = await operatorDisplayedTest;

            Assert.True(res1);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_WhenWrongCommandSend_ShouldCallHandler()
        {
            var wrongCommandCalled = false;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2($"127.0.0.1:22225");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2($"127.0.0.1:22226");
            remote.ApiMap.RegisterWrongCommandHandler(() =>
            {
                wrongCommandCalled = true;
            });
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22226");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");
            await apiOperator.RaiseCommandReceived("withparameters param1");
            // wait command complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 4);
            //--
            
            Assert.True(wrongCommandCalled);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_WhenCommandSendAndDelayedResponse_ShouldCompleteSendingAfterTimeout()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2($"127.0.0.1:22227");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2($"127.0.0.1:22228");
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22228");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");

            var remoteAccept1 = remote.GetSocketByTag("accept_1");
            remoteAccept1.UpdatedBefore += (substitution, line, methodName) =>
            {
                if (methodName.Equals("Send"))
                {
                    line.Value = () => throw new Exception("exception");
                }
            };
            //IntegrationTestsHelpers.ArrangeDelayForSocketExecution(remote, "accept_1");
            await apiOperator.RaiseCommandReceived("one");

            await Task.Delay(9000);
            //Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            //Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
        
        [Fact]
        public async void Operator_WhenCommandWithParametersSend_ShouldGetParameters2()
        {
            var actualParameter = string.Empty;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets2($"127.0.0.1:22223");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets2($"127.0.0.1:22224");
            remote.ApiMap.RegisterCommandWithParameters("withparameters", prm =>
            {
                actualParameter = prm.FirstOrDefault();
            });
            
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22224");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");
            await apiOperator.RaiseCommandReceived("withparameters param1");
            // wait command complete
            var connect2 = apiOperator.GetSocketByTag("connect_2");
            await connect2.ArrangeWaiting(connect2.ReceiveCalledTimes, 4);
            //--
            
            Assert.Equal("param1", actualParameter);
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
    }
}