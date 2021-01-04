using System;
using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Factories;
using RemoteApi.Monitor;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi.Integration
{
    public class RemoteApiIntegrationTests
    {
        private readonly IRecorder _recorder;
        private readonly IApplicationRecorder _applicationRecorder;
        
        private readonly IMonitoredRemoteOperator _mro;
        private readonly ITraceableRemoteApiMap _tram;

        public RemoteApiIntegrationTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            
            // MonitoredRemoteOperator
            var instructionSenderFactory = new TestInstructionSenderFactory(SocketFactory, _recorder);
            var monitoredRemoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(
                instructionSenderFactory, 
                Mock.Create<IRemoteTraceMonitor>(), _recorder);

            var monitoredRemoteOperator = monitoredRemoteOperatorFactory.Create("216.58.215.78:8080");
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(SocketFactory, _recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                _applicationRecorder);

            var traceableRemoteApiMap = traceableRemoteApiMapFactory.Create("216.58.215.78:8080");
            monitoredRemoteOperator.Start();
        }
        
        [Fact]
        public void Integration()
        {
            Assert.Equal(1, 1);
        }
        
        private ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new SocketTester();
        }

        /*
        private IInstructionSenderFactory CreateInstructionSender(ISocketFactory socketFactory)
        {
        }
        */
    }
}