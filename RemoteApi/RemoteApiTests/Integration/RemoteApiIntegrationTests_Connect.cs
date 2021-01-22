using System.Collections.Generic;
using RemoteApi.Integration.Helpers;
using Xunit;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        [Fact]
        public async void Operator_WhenCalledConnectWithotParameters_ShouldCatchError()
        {
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            ArrangeLocalOperatorWithSockets(remoteTraceMonitorСonsistent);
            await remoteTraceMonitorСonsistent.Initialized;

            await remoteTraceMonitorСonsistent.RaiseCommandReceived("connect");
            
            LogCacheRecorderTestInfo();
            Assert.Equal(1, _cacheRecorder.RecordErrorCalledTimes);
        }

        private (IApiOperator, Dictionary<string, SocketTester>) ArrangeLocalOperatorWithSockets(RemoteTraceMonitorСonsistentTester remoteTraceMonitorСonsistent)
        {
            var sockets = new Dictionary<string, SocketTester>();
            var socketTester1 = new SocketTester("connector");
            var socketTester2 = new SocketTester("listener");
            var socketTester3 = new SocketTester("accepted");
            var socketFactory = ArrangeConnection(socketTester1, socketTester2, socketTester3);
            var apiOperator = CreateLocalOperator(socketFactory, _cacheRecorder, remoteTraceMonitorСonsistent);
            
            sockets.Add("connector", socketTester1);
            sockets.Add("listener", socketTester2);
            sockets.Add("accepted", socketTester3);

            return (apiOperator, sockets);
        }

        private void LogCacheRecorderTestInfo()
        {
            _output.WriteLine("Errors:");
            _output.WriteLine(_cacheRecorder.ErrorCache);
            _output.WriteLine("Info:");
            _output.WriteLine(_cacheRecorder.InfoCache);
        }
    }
}