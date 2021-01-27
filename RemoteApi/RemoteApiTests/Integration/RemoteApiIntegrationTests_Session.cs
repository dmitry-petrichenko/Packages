using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Factories;
using RemoteApi.Integration.Helpers;
using Xunit;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        [Fact]
        public async void Operator_WhenRemoteSocketDisconnected_ShouldConnectLocal()
        {
            var apiOperator = ArrangeLocalOperatorTestWrapperRealSockets("127.0.0.1:11111");
            var remote = ArrangeRemoteApiMapTestWrapperWithRealSockets("127.0.0.1:22222");
            await apiOperator.Initialized;

            await apiOperator.RaiseCommandReceived("connect 127.0.0.1:22222");

            await Task.Delay(1000);
            
            remote.Sockets[0].Close();
            
            await Task.Delay(1000);
            
            LogCacheRecorderTestInfo(apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            LogCacheRecorderTestInfo(remote.Recorder);
            Assert.Equal(1, 1);
        }
        
        private TraceableRemoteApiMapWrapperRealSockets ArrangeRemoteApiMapTestWrapperWithRealSockets(
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            var sockets = new List<ISocket>();

            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory = (family, type, arg3) =>
            {
                var socket = new SocketAbstraction(family, type, arg3);
                sockets.Add(socket);
                return socket;
            };
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                recorder);
            
            var apiMap = traceableRemoteApiMapFactory.Create(address);

            return new TraceableRemoteApiMapWrapperRealSockets(sockets, apiMap, recorder);
        }
        
        private RemoteOperatorTestWrapperRealSockets ArrangeLocalOperatorTestWrapperRealSockets(
            string address = "127.0.0.1:22222")
        {
            var recorder = new ApplicationCacheRecorder();
            var sockets = new List<ISocket>();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();

            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory = (family, type, arg3) =>
            {
                var socket = new SocketAbstraction(family, type, arg3);
                sockets.Add(socket);
                return socket;
            };
            
            var apiOperator = CreateLocalOperator(socketFactory, recorder, remoteTraceMonitorСonsistent, address);

            return new RemoteOperatorTestWrapperRealSockets(sockets, apiOperator, recorder, remoteTraceMonitorСonsistent);
        }
    }
}