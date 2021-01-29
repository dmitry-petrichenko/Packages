﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Factories;
using RemoteApi.Monitor;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit.Abstractions;

namespace RemoteApi.Integration2.Helpers
{
    public class IntegrationTestsHelpers
    {
        internal static RemoteOperatorTestWrapperRealSockets ArrangeLocalOperatorTestWrapperRealSockets(
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
        
        internal static TraceableRemoteApiMapWrapperRealSockets ArrangeRemoteApiMapTestWrapperWithRealSockets(
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
        
        internal static IApiOperator CreateLocalOperator(
            Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory,
            ApplicationCacheRecorder recorder,
            IRemoteTraceMonitorСonsistent remoteTraceMonitor = default,
            string address = "127.0.0.0:11111")
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
                recorder);
            
            var apiOperatorFactory = new ApiOperatorFactory(recorder, monitoredRemoteOperatorFactory, traceableRemoteApiMapFactory, recorder);
            return apiOperatorFactory.Create(address);
        }
        
        internal static void LogCacheRecorderTestInfo(ITestOutputHelper output, ApplicationCacheRecorder recorder)
        {
            output.WriteLine("Sysytem Errors:");
            output.WriteLine(recorder.SystemErrorCache);
            output.WriteLine("Sysytem Info:");
            output.WriteLine(recorder.SystemInfoCache);
            output.WriteLine("Application Error:");
            output.WriteLine(recorder.AppErrorCache);
            output.WriteLine("Application Info:");
            output.WriteLine(recorder.AppInfoCache);
            output.WriteLine("Interrupt messages:");
            output.WriteLine(recorder.InterruptMessagesCache);
        }
    }
    
    internal class RemoteOperatorTestWrapperRealSockets
    {
        public IReadOnlyList<SocketTesterWrapper> Sockets { get; }
        public IApiOperator Operator { get; }
        public ApplicationCacheRecorder Recorder { get; }
        public Task MessageDisplayed => _remoteTraceMonitorСonsistentTester.MessageDisplayed;

        private readonly RemoteTraceMonitorСonsistentTester _remoteTraceMonitorСonsistentTester;
            
        public RemoteOperatorTestWrapperRealSockets(IReadOnlyList<SocketTesterWrapper> sockets, IApiOperator @operator, ApplicationCacheRecorder recorder, RemoteTraceMonitorСonsistentTester remoteTraceMonitorСonsistentTester)
        {
            Sockets = sockets;
            Operator = @operator;
            Recorder = recorder;
            _remoteTraceMonitorСonsistentTester = remoteTraceMonitorСonsistentTester;
        }

        public Task<bool> RaiseCommandReceived(string value)
        {
            return _remoteTraceMonitorСonsistentTester.RaiseCommandReceived(value);
        }
            
        public Task Initialized => _remoteTraceMonitorСonsistentTester.Initialized;
    }
    
    internal class TraceableRemoteApiMapWrapperRealSockets
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
    
    
}