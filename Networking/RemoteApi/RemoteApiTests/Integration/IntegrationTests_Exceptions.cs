using System;
using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    [Collection("Serial")]
    public class IntegrationTests_Exceptions
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_Exceptions(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async void Operator_WhenCommandSend_ShouldDisplayInMonitor2()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets($"127.0.0.1:22231");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets($"127.0.0.1:22232");

            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22232");
            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_2");

            var connect2 = apiOperator.GetSocketByTag("connect_2");
            connect2.UpdatedAfter += (substitution, line, tag) =>
            {
                if (substitution.SendCalledTimes.Value == 3)
                {
                    throw new Exception("ex");
                }
            };

            apiOperator.RaiseCommandReceived("cmd");
            
            //await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_3");
            await Task.Delay(1000);
            //Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            //Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
    }
}