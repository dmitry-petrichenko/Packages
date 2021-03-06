﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Operator;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi.Integration.Helpers;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        [Fact]
        public async void Operator_WhenCalledConnectWithotParameters_ShouldCatchError()
        {
            var local = ArrangeLocalOperatorTestWrapper();
            await local.Initialized;

            await local.RaiseCommandReceived("connect");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            Assert.Equal(1, local.Recorder.SystemErrorCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithIncorrectParameters_ShouldCatchError()
        {
            var local = ArrangeLocalOperatorTestWrapper();
            await local.Initialized;

            await local.RaiseCommandReceived("connect 123.3324234");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            Assert.Equal(1, local.Recorder.SystemErrorCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithNotExistingAddress_ShouldCatchError()
        {
            var socket = new SocketTester();
            socket.ConnectAction = (address, i) => throw new Exception("Connect exception");
            var local = ArrangeLocalOperatorTestWrapper(new [] { socket });
            await local.Initialized;

            await local.RaiseCommandReceived("connect 123.0.0.2:2334");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            Assert.Equal(1, socket.ConnectCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenCalledConnectWithExistingAddress_ShouldConnect()
        {
            var socket = new SocketTester("remote");
            var operatorApi = ArrangeLocalOperatorTestWrapper(
                new [] { socket },
                "111.111.111.111:11111");
            
            var remote = ArrangeRemoteApiMapWithSocketsAndRecorders(socket, "222.222.222.222:2222");
            await operatorApi.Initialized;
            operatorApi.Recorder.ClearCache();

            await operatorApi.RaiseCommandReceived("connect 222.222.222.222:2222");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, operatorApi.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, socket.ConnectCalledTimes);
        }
        
        [Fact]
        public async void Operator_WhenSendWrongCommandToRemote_ShouldCallWrongCommand()
        {
            bool wrongCommandWasCalled = false;
            var socket = new SocketTester("remote");
            socket.SetLocalEndPoint(IPAddress.Parse("111.111.111.111"), 11111);
            var local = ArrangeLocalOperatorTestWrapper(
                new [] { socket },
                "111.111.111.111:11111");
            
            var remote = ArrangeRemoteApiMapWithSocketsAndRecorders(socket, "222.222.222.222:2222");
            remote.ApiMap.RegisterWrongCommandHandler(() => wrongCommandWasCalled = true);
            await local.Initialized;
            await local.RaiseCommandReceived("connect 222.222.222.222:2222");
            local.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            await local.RaiseCommandReceived("hello");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.True(wrongCommandWasCalled);
        }
        
        [Fact]
        public async void Operator_WhenSendCommandToRemote_ShouldReceiveCommand()
        {
            int command1ReceivedTimes = 0;
            int command2ReceivedTimes = 0;
            int command3ReceivedTimes = 0;
            var socket = new SocketTester("remote");
            var local = ArrangeLocalOperatorTestWrapper(
                new [] { socket },
                "111.111.111.111:11111");
            
            var remote = ArrangeRemoteApiMapWithSocketsAndRecorders(socket, "222.222.222.222:2222");
            remote.ApiMap.RegisterCommand("command1", () => command1ReceivedTimes++);
            remote.ApiMap.RegisterCommand("command2", () => command2ReceivedTimes++);
            remote.ApiMap.RegisterCommand("command3", () => command3ReceivedTimes++);
            await local.Initialized;
            await local.RaiseCommandReceived("connect 222.222.222.222:2222");
            local.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            await local.RaiseCommandReceived("command1");
            await local.RaiseCommandReceived("command2");
            await local.RaiseCommandReceived("command2");
            await local.RaiseCommandReceived("command3");
            
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, command1ReceivedTimes);
            Assert.Equal(2, command2ReceivedTimes);
            Assert.Equal(1, command3ReceivedTimes);
        }
        
        [Fact]
        public async void Operator_WhenRemoteSocketDisconnected_ShouldDisposeSocket()
        {
            var socket = new SocketTester("remote");
            var local = ArrangeLocalOperatorTestWrapper(
                new [] { socket },
                "111.111.111.111:11111");
            
            var remote = ArrangeRemoteApiMapWithSocketsAndRecorders(socket, "222.222.222.222:2222");
            await local.Initialized;
            await local.RaiseCommandReceived("connect 222.222.222.222:2222");
            await remote.Connected;
            local.Recorder.ClearCache();
            remote.Recorder.ClearCache();

            socket.RaiseDisconnected();

            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, local.Recorder);
            _output.WriteLine("-----------------------------");
            IntegrationTestsHelpers.LogCacheRecorderTestInfo(_output, remote.Recorder);
            Assert.Equal(1, remote.Sockets["connecter"].DisposeCalledTimes);
        }

        private TraceableRemoteApiMapWrapper ArrangeRemoteApiMapWithSocketsAndRecorders(
            SocketTester socketConnecter,
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            var result = ArrangeRemoteApiMapWithSockets(recorder, recorder, socketConnecter, address);

            return new TraceableRemoteApiMapWrapper(result.Item2, result.Item1, recorder);
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
            var socketFactory = ArrangeSocketFactoryTraceableRemoteApiMap(socketConnecter, socketListener, socketAccepted, isRemote:true);
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                applicationRecorder);
            
            sockets.Add("connecter", socketConnecter);
            sockets.Add("listener", socketListener);
            sockets.Add("accepted", socketAccepted);

            var apiMap = traceableRemoteApiMapFactory.Create(address);
            return (apiMap, sockets);
        }
        
        private Func<AddressFamily, SocketType, ProtocolType, string, ISocket> ArrangeSocketFactoryTraceableRemoteApiMap(
            SocketTester socketConnecter, 
            SocketTester socketListener, 
            SocketTester socketAccepted, 
            IEnumerable<SocketTester> otherSockets = default,
            bool isRemote = false)
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
                if (isRemote)
                {
                    socketAccepted.SetRemoteEndPoint(socketConnecter.LocalEndPoint.Address, address);
                    socketAccepted.SetLocalEndPoint(ip, 6666);
                }
                else
                {
                    socketAccepted.SetRemoteEndPoint(ip, address);
                    socketAccepted.SetLocalEndPoint(socketConnecter.LocalEndPoint.Address, 6666);
                }
                socketListener.RaiseSocketAccepted(socketAccepted);
            };
            
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
            {
                return socketTesterFactory.Create();
            }
            
            return SocketFactory;
        }

        private RemoteOperatorTestWrapperFakeSockets ArrangeLocalOperatorTestWrapper(
            IEnumerable<SocketTester> otherSockets = default,
            string address = "111.111.111.111:11111")
        {
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorConsistentTester(null);
            var addressAndPort = address.Split(":");
            var recorder = new ApplicationCacheRecorder();
            var sockets = new Dictionary<string, SocketTester>();
            var socketTester1 = new SocketTester("connecter");
            socketTester1.SetLocalEndPoint(IPAddress.Parse(addressAndPort[0]), Convert.ToInt32(addressAndPort[1]));
            var socketTester2 = new SocketTester("listener");
            var socketTester3 = new SocketTester("accepted");

            if (otherSockets != default)
            {
                foreach (var socketTester in otherSockets)
                {
                    socketTester.SetLocalEndPoint(IPAddress.Parse(addressAndPort[0]), Convert.ToInt32(addressAndPort[1]));
                }
            }
            
            var socketFactory = ArrangeSocketFactoryLocal(
                socketTester1, 
                socketTester2, 
                socketTester3, 
                otherSockets);
            var apiOperator = CreateLocalOperator(socketFactory, recorder, remoteTraceMonitorСonsistent, address);
            
            sockets.Add("connector", socketTester1);
            sockets.Add("listener", socketTester2);
            sockets.Add("accepted", socketTester3);

            return new RemoteOperatorTestWrapperFakeSockets(sockets, apiOperator, recorder, remoteTraceMonitorСonsistent);
        }

        private class RemoteOperatorTestWrapperFakeSockets
        {
            public Dictionary<string, SocketTester> Sockets { get; }
            public IApiOperator Operator { get; }
            public ApplicationCacheRecorder Recorder { get; }

            private readonly RemoteTraceMonitorConsistentTester _remoteTraceMonitorConsistentTester;
            
            public RemoteOperatorTestWrapperFakeSockets(Dictionary<string, SocketTester> sockets, IApiOperator @operator, ApplicationCacheRecorder recorder, RemoteTraceMonitorConsistentTester remoteTraceMonitorConsistentTester)
            {
                Sockets = sockets;
                Operator = @operator;
                Recorder = recorder;
                _remoteTraceMonitorConsistentTester = remoteTraceMonitorConsistentTester;
            }

            public Task<bool> RaiseCommandReceived(string value)
            {
                return _remoteTraceMonitorConsistentTester.RaiseCommandReceived(value);
            }
            
            public Task Initialized => _remoteTraceMonitorConsistentTester.Initialized;
        }
        
        private class RemoteOperatorTestWrapperRealSockets
        {
            public IReadOnlyList<SocketTesterWrapper> Sockets { get; }
            public IApiOperator Operator { get; }
            public ApplicationCacheRecorder Recorder { get; }

            private readonly RemoteTraceMonitorConsistentTester _remoteTraceMonitorConsistentTester;
            
            public RemoteOperatorTestWrapperRealSockets(IReadOnlyList<SocketTesterWrapper> sockets, IApiOperator @operator, ApplicationCacheRecorder recorder, RemoteTraceMonitorConsistentTester remoteTraceMonitorConsistentTester)
            {
                Sockets = sockets;
                Operator = @operator;
                Recorder = recorder;
                _remoteTraceMonitorConsistentTester = remoteTraceMonitorConsistentTester;
            }

            public Task<bool> RaiseCommandReceived(string value)
            {
                return _remoteTraceMonitorConsistentTester.RaiseCommandReceived(value);
            }
            
            public Task Initialized => _remoteTraceMonitorConsistentTester.Initialized;
        }
        
        private class TraceableRemoteApiMapWrapperRealSockets
        {
            public IReadOnlyList<SocketTesterWrapper> Sockets { get; }
            public ITraceableRemoteApiMap ApiMap { get; }
            public ApplicationCacheRecorder Recorder { get; }
            public Task Connected => _connectedTask.Task;

            private TaskCompletionSource<bool> _connectedTask;

            public TraceableRemoteApiMapWrapperRealSockets(IReadOnlyList<SocketTesterWrapper> sockets, ITraceableRemoteApiMap apiMap, ApplicationCacheRecorder recorder)
            {
                Sockets = sockets;
                ApiMap = apiMap;
                Recorder = recorder;
                _connectedTask = new TaskCompletionSource<bool>();

                ((TraceableRemoteApiMap) ApiMap).TraceStarted += TraceStartedHandler;
            }

            private void TraceStartedHandler()
            {
                _connectedTask.SetResult(true);
            }
        }

        private class TraceableRemoteApiMapWrapper
        {
            public Dictionary<string, SocketTester> Sockets { get; }
            public ITraceableRemoteApiMap ApiMap { get; }
            public ApplicationCacheRecorder Recorder { get; }
            public Task Connected => _connectedTask.Task;

            private TaskCompletionSource<bool> _connectedTask;

            public TraceableRemoteApiMapWrapper(Dictionary<string, SocketTester> sockets, ITraceableRemoteApiMap apiMap, ApplicationCacheRecorder recorder)
            {
                Sockets = sockets;
                ApiMap = apiMap;
                Recorder = recorder;
                _connectedTask = new TaskCompletionSource<bool>();

                if (Sockets.TryGetValue("accepted", out SocketTester socket))
                {
                    socket.Byte49ReceivedFirstTime += SocketOnByte49ReceivedFirstTime;
                }
            }

            private void SocketOnByte49ReceivedFirstTime()
            {
                _connectedTask.SetResult(true);
            }
        }
    }
}