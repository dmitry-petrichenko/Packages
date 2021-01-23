using System;
using System.Collections.Generic;
using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Factories;
using RemoteApi.Integration.Helpers;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        [Fact]
        public async void Operator_WhenCalledConnectWithotParameters_ShouldCatchError()
        {
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            var local = ArrangeLocalOperatorWithSocketsAndRecorder(remoteTraceMonitorСonsistent);
            await remoteTraceMonitorСonsistent.Initialized;

            await remoteTraceMonitorСonsistent.RaiseCommandReceived("connect");
            
            LogCacheRecorderTestInfo(local.Item3);
            Assert.Equal(1, local.Item3.SystemErrorCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithIncorrectParameters_ShouldCatchError()
        {
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            var local = ArrangeLocalOperatorWithSocketsAndRecorder(remoteTraceMonitorСonsistent);
            await remoteTraceMonitorСonsistent.Initialized;

            await remoteTraceMonitorСonsistent.RaiseCommandReceived("connect 123.3324234");
            
            LogCacheRecorderTestInfo(local.Item3);
            Assert.Equal(1, local.Item3.SystemErrorCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithNotExistingAddress_ShouldCatchError()
        {
            var socket = new SocketTester();
            socket.ConnectAction = (address, i) => throw new Exception("Connect exception");
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            var local = ArrangeLocalOperatorWithSocketsAndRecorder(remoteTraceMonitorСonsistent, new [] { socket });
            await remoteTraceMonitorСonsistent.Initialized;

            await remoteTraceMonitorСonsistent.RaiseCommandReceived("connect 123.0.0.2:2334");
            
            LogCacheRecorderTestInfo(local.Item3);
            Assert.Equal(1, socket.ConnectCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithExistingAddress_ShouldConnect()
        {
            var socket = new SocketTester();
            var remote = ArrangeRemoteApiMapWithSocketsAndRecorders(socket, "222.222.222.222:1001");
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            var local = ArrangeLocalOperatorWithSocketsAndRecorder(remoteTraceMonitorСonsistent, new [] { socket });
            await remoteTraceMonitorСonsistent.Initialized;
            local.Item3.ClearCache();

            await remoteTraceMonitorСonsistent.RaiseCommandReceived("connect 222.222.222.222:1001");
            
            LogCacheRecorderTestInfo(local.Item3);
            _output.WriteLine("-----------------------------");
            LogCacheRecorderTestInfo(remote.Item3);
            Assert.Equal(1, socket.ConnectCalledTimes);
        }

        private (ITraceableRemoteApiMap, Dictionary<string, SocketTester>, ApplicationCacheRecorder) ArrangeRemoteApiMapWithSocketsAndRecorders(
            SocketTester socketConnecter,
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            var result = ArrangeRemoteApiMapWithSockets(recorder, recorder, socketConnecter, address);

            return (result.Item1, result.Item2, recorder);
        }

        private (ITraceableRemoteApiMap, Dictionary<string, SocketTester>) ArrangeRemoteApiMapWithSockets(
            IApplicationRecorder applicationRecorder,
            IRecorder recorder, 
            SocketTester socketConnecter, 
            string address)
        {
            var sockets = new Dictionary<string, SocketTester>();
            var socketListener = new SocketTester("listener");
            var socketAccepted = new SocketTester("accepted");
            var socketFactory = ArrangeSocketFactoryTraceableRemoteApiMap(socketConnecter, socketListener, socketAccepted);
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                applicationRecorder);
            
            sockets.Add("connector", socketConnecter);
            sockets.Add("listener", socketListener);
            sockets.Add("accepted", socketAccepted);

            var apiMap = traceableRemoteApiMapFactory.Create(address);
            return (apiMap, sockets);
        }
        
        private Func<AddressFamily, SocketType, ProtocolType, ISocket> ArrangeSocketFactoryTraceableRemoteApiMap(
            SocketTester socketConnecter, 
            SocketTester socketListener, 
            SocketTester socketAccepted, IEnumerable<SocketTester> otherSockets = default)
        {
            socketAccepted.Connected = true;
            
            new NetworkImitator(socketConnecter, socketAccepted);
            
            var socketTesterFactory = Mock.Create<ISocketTesterFactory>();
            Mock.Arrange(() => socketTesterFactory.Create()).Returns(socketListener)
                .InSequence();

            if (otherSockets != default)
            {
                foreach (var socket in otherSockets)
                {
                    Mock.Arrange(() => socketTesterFactory.Create()).Returns(socket)
                        .InSequence();
                }
            }

            socketConnecter.ConnectCalled += (ip, address) =>
            {
                socketAccepted.SetRemoteEndPoint(ip, address);
                socketAccepted.SetLocalEndPoint(socketConnecter.LocalEndPoint.Address, 6666);
                socketListener.RaiseSocketAccepted(socketAccepted);
            };
            
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                return socketTesterFactory.Create();
            }
            
            return SocketFactory;
        }

        private (IApiOperator, Dictionary<string, SocketTester>, ApplicationCacheRecorder) ArrangeLocalOperatorWithSocketsAndRecorder(
            RemoteTraceMonitorСonsistentTester remoteTraceMonitorСonsistent,
            IEnumerable<SocketTester> otherSockets = default)
        {
            var recorder = new ApplicationCacheRecorder();
            var sockets = new Dictionary<string, SocketTester>();
            var socketTester1 = new SocketTester("connector");
            var socketTester2 = new SocketTester("listener");
            var socketTester3 = new SocketTester("accepted");
            var socketFactory = ArrangeSocketFactoryLocal(socketTester1, socketTester2, socketTester3, otherSockets);
            var apiOperator = CreateLocalOperator(socketFactory, recorder, remoteTraceMonitorСonsistent);
            
            sockets.Add("connector", socketTester1);
            sockets.Add("listener", socketTester2);
            sockets.Add("accepted", socketTester3);

            return (apiOperator, sockets, recorder);
        }

        private void LogCacheRecorderTestInfo(ApplicationCacheRecorder recorder)
        {
            _output.WriteLine("Sysytem Errors:");
            _output.WriteLine(recorder.SystemErrorCache);
            _output.WriteLine("Sysytem Info:");
            _output.WriteLine(recorder.SystemInfoCache);
            _output.WriteLine("Application Error:");
            _output.WriteLine(recorder.AppErrorCache);
            _output.WriteLine("Application Info:");
            _output.WriteLine(recorder.AppInfoCache);
        }
    }
}