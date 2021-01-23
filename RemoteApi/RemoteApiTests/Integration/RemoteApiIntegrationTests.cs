using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Factories;
using RemoteApi.Integration.Helpers;
using RemoteApi.Monitor;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        private readonly IRecorder _recorder;
        private readonly IRecorder _originalRecorder;
        private readonly CacheRecorder _cacheRecorder;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly ISystemRecorder _systemRecorder;
        private readonly ITestOutputHelper _output;
        
        private readonly IMonitoredRemoteOperator _mro;
        private readonly ITraceableRemoteApiMap _tram;

        public RemoteApiIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
            _originalRecorder = new DefaultRecorder(new DefaultRecorderSettings());
            _cacheRecorder = new CacheRecorder();
            _recorder = Mock.Create<IRecorder>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            _systemRecorder = Mock.Create<ISystemRecorder>();
        }

        private IApiOperator CreateLocalOperator(
            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory,
            IRecorder recorder,
            IRemoteTraceMonitorСonsistent remoteTraceMonitor = default)
        {
            if (remoteTraceMonitor == default)
            {
                remoteTraceMonitor = Mock.Create<IRemoteTraceMonitorСonsistent>();
            }
            
            // MonitoredRemoteOperator
            var instructionSenderFactory = new TestInstructionSenderFactory(socketFactory, recorder);
            var monitoredRemoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(
                instructionSenderFactory, 
                remoteTraceMonitor, recorder);

            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                _applicationRecorder);
            
            var apiOperatorFactory = new ApiOperatorFactory(_systemRecorder, monitoredRemoteOperatorFactory, traceableRemoteApiMapFactory, _applicationRecorder);
            return apiOperatorFactory.Create("216.58.215.78:8080");
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
            
            CreateLocalOperator(SocketFactory, _recorder);
            
            Assert.Equal(2, socketCreated);
        }
        
        [Fact]
        public void OperatorConstructor_WhenCalled_ShouldListenFirstSocket()
        {
            var socketTesterFactory = Mock.Create<ISocketTesterFactory>();
            var socketTester1 = new SocketTester();
            var socketTester2 = new SocketTester();
            
            Mock.Arrange(() => socketTesterFactory.Create()).Returns(socketTester1)
                .InSequence(); 
            Mock.Arrange(() => socketTesterFactory.Create()).Returns(socketTester2)
                .InSequence();
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                return socketTesterFactory.Create();
            }
            
            CreateLocalOperator(SocketFactory, _recorder);
            
            Assert.Equal(1, socketTester1.ListenCalledTimes);
            Assert.Equal(0, socketTester2.ListenCalledTimes);
            Assert.Equal(1, socketTester2.ConnectCalledTimes);
        }
        
        [Fact]
        public async void OperatorConstructor_WhenCalled_ShouldCallReceiveInAcceptedSocket()
        {
            var socketTester1 = new SocketTester("connector");
            var socketTester2 = new SocketTester("listener");
            var socketTester3 = new SocketTester("accepted");
            var socketFactory = ArrangeSocketFactoryLocal(socketTester1, socketTester2, socketTester3);
            
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            CreateLocalOperator(socketFactory, _cacheRecorder, remoteTraceMonitorСonsistent);

            await remoteTraceMonitorСonsistent.Initialized;
            
            _output.WriteLine("Errors:");
            _output.WriteLine(_cacheRecorder.ErrorCache);
            _output.WriteLine("Info:");
            _output.WriteLine(_cacheRecorder.InfoCache);
            Assert.Equal(3, socketTester3.ReceiveCalledTimes);
            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), 
                Occurs.Never());
            Mock.Assert(() => _recorder.RecordError(Arg.IsAny<string>(), Arg.IsAny<string>()), 
                Occurs.Never());
        }

        private Func<AddressFamily, SocketType, ProtocolType, ISocket> ArrangeSocketFactoryLocal(
            SocketTester socketConnecter, 
            SocketTester socketListener, 
            SocketTester socketAccepted, IEnumerable<SocketTester> otherSockets = default)
        {
            socketAccepted.Connected = true;
            
            new NetworkImitator(socketConnecter, socketAccepted);
            
            var socketTesterFactory = Mock.Create<ISocketTesterFactory>();
            Mock.Arrange(() => socketTesterFactory.Create()).Returns(socketListener)
                .InSequence();
            Mock.Arrange(() => socketTesterFactory.Create()).Returns(socketConnecter)
                .InSequence();

            if (otherSockets != default)
            {
                foreach (var socket in otherSockets)
                {
                    Mock.Arrange(() => socketTesterFactory.Create()).Returns(socket)
                        .InSequence();
                }
            }

            socketConnecter.ConnectCalled += (ip, port) =>
            {
                socketListener.RaiseSocketAccepted(socketAccepted);
            };
            
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                return socketTesterFactory.Create();
            }
            
            return SocketFactory;
        }
    }
}