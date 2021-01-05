using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        private readonly ISystemRecorder _systemRecorder;
        
        private readonly IMonitoredRemoteOperator _mro;
        private readonly ITraceableRemoteApiMap _tram;

        public RemoteApiIntegrationTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            _systemRecorder = Mock.Create<ISystemRecorder>();
        }

        private void CreateLocalOperator(Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory)
        {
            // MonitoredRemoteOperator
            var instructionSenderFactory = new TestInstructionSenderFactory(socketFactory, _recorder);
            var monitoredRemoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(
                instructionSenderFactory, 
                Mock.Create<IRemoteTraceMonitor>(), _recorder);

            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, _recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                _applicationRecorder);
            
            var apiOperatorFactory = new ApiOperatorFactory(_systemRecorder, monitoredRemoteOperatorFactory, traceableRemoteApiMapFactory, _applicationRecorder);
            apiOperatorFactory.Create("216.58.215.78:8080");
        }

        [Fact]
        public async void NetworkImitatorReceiveTestLoop()
        {
            var bytesToSend = new byte[] {0b00110000, 0b00110011};
            var bytesReceived = new byte[1024];
            var size = 0;
            var socket1 = new SocketTester();
            var socket2 = new SocketTester();
            var networkImitator = new NetworkImitator(socket1, socket2);

            for (int i = 0; i < 3; i++)
            {
                var receivedTask = new TaskCompletionSource<bool>();
                bytesReceived = new byte[1024];
                Task.Run(async () =>
                {
                    size = socket2.Receive(bytesReceived);
                    receivedTask.SetResult(true);
                });

                await Task.Delay(500);
                socket1.Send(bytesToSend);
                await receivedTask.Task;
            
                var pureReceived = bytesReceived.Take(size).ToArray();
                Assert.Equal(bytesToSend, pureReceived);
            }
        }
        
        [Fact]
        public async void NetworkImitatorDisconnectedTest()
        {
            var receivedTask = new TaskCompletionSource<bool>();
            var bytesReceived = new byte[1024];
            var size = 0;
            var socket2 = new SocketTester();

            bytesReceived = new byte[1024];
            Task.Run(async () =>
            { 
                socket2.Connect(new IPAddress(0), 0);
                size = socket2.Receive(bytesReceived);
                receivedTask.SetResult(true);
            });

            await Task.Delay(500);
            socket2.RaiseDisconnected();
            await receivedTask.Task;
            
            Assert.Equal(0, size);
            Assert.False(socket2.Connected);
        }
        
        [Fact]
        public void OperatorConstructor_WhenCalled_ShouldCreateTwoSockets()
        {
            var socketCreated = 0;
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                socketCreated++;
                return new SocketTester();
            }
            
            CreateLocalOperator(SocketFactory);
            
            Assert.Equal(2, socketCreated);
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