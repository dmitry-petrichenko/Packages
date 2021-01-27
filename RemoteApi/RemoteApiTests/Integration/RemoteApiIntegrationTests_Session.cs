using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
            
            await remote.Connected;

            LogCacheRecorderTestInfo(apiOperator.Recorder);
            _output.WriteLine("-----------------------------");
            LogCacheRecorderTestInfo(remote.Recorder);
            Assert.Equal(1, apiOperator.Sockets[1].CloseCalledTimes);
        }
        
        private TraceableRemoteApiMapWrapperRealSockets ArrangeRemoteApiMapTestWrapperWithRealSockets(
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            var sockets = new List<SocketTesterWrapper>();
            var socketFactoryCounter = 0;

            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory = (family, type, arg3) =>
            {
                socketFactoryCounter++;
                var socket = new SocketTesterWrapper(family, type, arg3, $"{socketFactoryCounter}");
                socket.Accepted += OnAccepted;
                sockets.Add(socket);
                return socket;
            };

            void OnAccepted(SocketTesterWrapper wrapper)
            {
                sockets.Add(wrapper);
            }
            
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
            var sockets = new List<SocketTesterWrapper>();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistentTester();
            var socketFactoryCounter = 0;
            
            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory = (family, type, arg3) =>
            {
                socketFactoryCounter++;
                var socket = new SocketTesterWrapper(family, type, arg3, $"{socketFactoryCounter}");
                socket.Accepted += OnAccepted;
                sockets.Add(socket);
                return socket;
            };
            
            void OnAccepted(SocketTesterWrapper wrapper)
            {
                sockets.Add(wrapper);
            }
            
            var apiOperator = CreateLocalOperator(socketFactory, recorder, remoteTraceMonitorСonsistent, address);

            return new RemoteOperatorTestWrapperRealSockets(sockets, apiOperator, recorder, remoteTraceMonitorСonsistent);
        }
    }
}