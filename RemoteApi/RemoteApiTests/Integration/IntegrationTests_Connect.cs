using System.Threading;
using C8F2740A.Common.Records;
using RemoteApi.Integration.Helpers;
using RemoteApi.Integration.Helpers.SocketsSubstitution;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    [Collection("Serial")]
    public class IntegrationTests_Connect
    {
        private ITestOutputHelper _output;
        
        public IntegrationTests_Connect(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void Operator_WhenConnectToRemoteSocket_ShouldDisconnectLocal()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11111");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:11112");
            
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
        public async void Operator_WhenRemoteApiMapIsResponsingSlower_ShouldWorkCorrect()
        {
            var wrongCommandReceived = false;
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11113");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:11114");
            remote.ApiMap.RegisterWrongCommandHandler(() => wrongCommandReceived = true);

            await IntegrationTestsHelpers.AssertConnectComplete(apiOperator, "connect_1");
            
            // arrange slowpoke remote
            remote.ArrangeSocketByTagDuringCreation("accept_1", s =>
            {
                s.UpdatedBefore += (s, line, method) =>
                {
                    Thread.Sleep(3000);
                };
            });
            //
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11114");

            // wait receive complete
            var accept1 = remote.GetSocketByTag("accept_1");
            await accept1.ArrangeWaiting(accept1.ReceiveCalledTimes, 2);
            
            ((IRecorder)apiOperator.Recorder).RecordInfo("--", "send command now");
            await apiOperator.RaiseCommandReceived("hello");
            // wait receive command complete
            var res = await accept1.ArrangeWaiting(accept1.SendCalledTimes, 3);
            Assert.True(res);

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            
            // Assert
            Assert.Equal(0, apiOperator.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, apiOperator.Recorder.AppErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.SystemErrorCalledTimes);
            Assert.Equal(0, remote.Recorder.AppErrorCalledTimes);
            Assert.True(wrongCommandReceived);
        }

        [Fact]
        public async void Operator_WhenRemoteSocketDisconnectedAndLost_ShouldTryConnectAndShowMessage()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11115");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:11116");

            await IntegrationTestsHelpers.WaitingConnectComplete(apiOperator, "connect_1");
            
            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:11116");

            // wait receive complete
            var operatorAccept1 = remote.GetSocketByTag("accept_1");
            await operatorAccept1.ArrangeWaiting(operatorAccept1.ReceiveCalledTimes, 2);

            var remoteListen1 = remote.GetSocketByTag("listen_1");
            var remoteAccept1 = remote.GetSocketByTag("accept_1");
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();
            
            remoteListen1.Close();
            remoteAccept1.Close();

            // wait close
            var r1 = 
                await remoteListen1
                    .ArrangeWaiting(remoteListen1.CloseCalledTimes, 1, 4000);
            var r2 = 
                await remoteAccept1
                    .ArrangeWaiting(remoteAccept1.CloseCalledTimes, 1, 4000);
            Assert.True(r1);
            Assert.True(r2);
            
            apiOperator.Recorder.ClearCache();
            remote.Recorder.ClearCache();
            
            // wait connect again complete
            var operatorListen1 = apiOperator.GetSocketByTag("listen_1");
            var r3 = await operatorListen1.ArrangeWaiting(operatorListen1.AcceptAsyncCalledTimes, 2, 4000);
            Assert.True(r3);
            
            var operatorConnect4 = apiOperator.GetSocketByTag("connect_4");
            var r4 = await operatorConnect4.ArrangeWaiting(operatorConnect4.ReceiveCalledTimes, 2);
            Assert.True(r4);

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
        }
    }
}