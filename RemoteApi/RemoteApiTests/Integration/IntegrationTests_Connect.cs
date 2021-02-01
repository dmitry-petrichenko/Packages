﻿using RemoteApi.Integration.Helpers;
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
        
        public void Dispose()
        {
        }

        [Fact]
        public async void Operator_WhenConnectToRemoteSocket_ShouldDisconnectLocal()
        {
            var apiOperator = IntegrationTestsHelpers.ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11113");
            var remote = IntegrationTestsHelpers.ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22224");
            await apiOperator.MessageDisplayed;

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22224");

            await remote.ConnectedComplete;

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