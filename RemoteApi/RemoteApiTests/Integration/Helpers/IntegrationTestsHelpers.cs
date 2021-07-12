using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Operator;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Xunit.Abstractions;

namespace RemoteApi.Integration.Helpers
{
    public class IntegrationTestsHelpers
    {
        internal static RemoteOperatorTestWrapperRealSockets2 ArrangeLocalOperatorTestWrapperRealSockets2(
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            ((IApplicationRecorder)recorder).RecordInfo("system", "ready");
            var sockets = new List<SocketSubstitution>();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorConsistentTester(recorder);

            var factoryOfFactory = new FactoryOfSubstitutedSocketFactory(sockets);
            var socketFactory = factoryOfFactory.Create();
            
            var apiOperator = CreateLocalOperator(socketFactory, recorder, remoteTraceMonitorСonsistent, address);

            return new RemoteOperatorTestWrapperRealSockets2(sockets, apiOperator, recorder, remoteTraceMonitorСonsistent);
        }
        
        internal static TraceableRemoteApiMapWrapperRealSockets2 ArrangeRemoteApiMapTestWrapperWithRealSockets2(
            string address)
        {
            var recorder = new ApplicationCacheRecorder();
            ((IApplicationRecorder)recorder).RecordInfo("system", "ready");
            var sockets = new List<SocketSubstitution>();
            var apiMapWrapper = default(TraceableRemoteApiMapWrapperRealSockets2);

            var factoryOfFactory = new FactoryOfSubstitutedSocketFactory(sockets);
            var socketFactory = factoryOfFactory.Create();
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                recorder);
            
            var apiMap = traceableRemoteApiMapFactory.Create(address);

            apiMapWrapper = new TraceableRemoteApiMapWrapperRealSockets2(sockets, apiMap, recorder);
            
            return apiMapWrapper;
        }
        
        internal class BaseWrapperRealSockets
        {
            public IReadOnlyList<SocketSubstitution> Sockets { get; }
            public ApplicationCacheRecorder Recorder { get; }
        
            public BaseWrapperRealSockets(IReadOnlyList<SocketSubstitution> sockets, ApplicationCacheRecorder recorder)
            {
                Sockets = sockets;
                Recorder = recorder;
            }
            
            public SocketSubstitution GetSocketByTag(string tag)
            {
                var tagName = string.Empty;
                foreach (var socket in Sockets)
                {
                    tagName = socket.Tag.Split(":").Last();
                    if (tagName.Equals(tag))
                    {
                        return socket;
                    }
                }

                return default;
            }
        }

        internal class FactoryOfSubstitutedSocketFactory
        {
            private readonly Dictionary<string, int> _tags;
            private readonly List<SocketSubstitution> _sockets;
            
            public FactoryOfSubstitutedSocketFactory(List<SocketSubstitution> sockets)
            {
                _tags = new Dictionary<string, int>();
                _sockets = sockets;
            }

            public Func<AddressFamily, SocketType, ProtocolType, string, ISocket> Create()
            {
                ISocket SocketRegularFactory(AddressFamily family, SocketType type, ProtocolType protocolType)
                {
                    var socket = new SocketAbstraction(family, type, protocolType);
                    return socket;
                }

                ISocket SocketAcceptFactory(ISocket socket, string tag)
                {
                    var socketSubstitution = new SocketSubstitution(socket, tag);
                    _sockets.Add(socketSubstitution);
                
                    return socketSubstitution;
                }
            
                ISocket SocketFactory(AddressFamily family, SocketType type, ProtocolType protocolType, string tag)
                {
                    var socketName = String.Empty;
                    if (_tags.ContainsKey(tag))
                    {
                        _tags[tag]++;
                        socketName = $"{tag}_{_tags[tag]}";
                    }
                    else
                    {
                        _tags.Add(tag, 1);
                        socketName = $"{tag}_1";
                    }

                    var socket = new SocketSubstitution(SocketRegularFactory, SocketAcceptFactory, family, type, protocolType, socketName);
                    _sockets.Add(socket);
                    return socket;
                }

                return SocketFactory;
            }
        }
        
        //-----------------------------------------------------------
        
        internal static RemoteOperatorTestWrapperRealSockets ArrangeLocalOperatorTestWrapperRealSockets(
            string address = "127.0.0.1:22222")
        {
            var recorder = new ApplicationCacheRecorder();
            var sockets = new List<SocketTesterWrapper>();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorConsistentTester(recorder);
            var socketFactoryCounter = 0;
            
            Func<AddressFamily, SocketType, ProtocolType, string, ISocket> socketFactory = (family, type, arg3, tag) =>
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
            var apiMapWrapper = default(TraceableRemoteApiMapWrapperRealSockets);
            
            void OnAccepted(SocketTesterWrapper wrapper)
            {
                sockets.Add(wrapper);
                apiMapWrapper.SocketAcceptedHandler?.Invoke(wrapper);
            }
            
            Func<AddressFamily, SocketType, ProtocolType, string, ISocket> socketFactory = (family, type, arg3, tag) =>
            {
                socketFactoryCounter++;
                var socket = new SocketTesterWrapper(family, type, arg3, $"{socketFactoryCounter}");
                socket.Accepted += OnAccepted;
                sockets.Add(socket);
                return socket;
            };
            
            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                recorder);
            
            var apiMap = traceableRemoteApiMapFactory.Create(address);

            apiMapWrapper = new TraceableRemoteApiMapWrapperRealSockets(sockets, apiMap, recorder);
            
            return apiMapWrapper;
        }
        
        internal static IApiOperator CreateLocalOperator(
            Func<AddressFamily, SocketType, ProtocolType, string, ISocket> socketFactory,
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
                remoteTraceMonitor, recorder, recorder);

            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                recorder);
            
            var apiOperatorFactory = new ApiOperatorFactory(monitoredRemoteOperatorFactory, traceableRemoteApiMapFactory, recorder);
            return apiOperatorFactory.Create(address);
        }
        
        internal static void LogCacheRecorderTestInfo(ITestOutputHelper output, ApplicationCacheRecorder recorder)
        {
            output.WriteLine($"SYSTEM ERRORS ({recorder.SystemErrorCalledTimes}):");
            output.WriteLine(recorder.SystemErrorCache);
            output.WriteLine("SYSTEM INFO:");
            output.WriteLine(recorder.SystemInfoCache);
            output.WriteLine($"APPLICATION ERROR ({recorder.AppErrorCalledTimes}):");
            output.WriteLine(recorder.AppErrorCache);
            output.WriteLine("APPLICATION INFO:");
            output.WriteLine(recorder.AppInfoCache);
            output.WriteLine("MESSAGES ON DISPLAY:");
            output.WriteLine(recorder.DisplayMessagesCache);
        }
    }
    
    internal class RemoteOperatorTestWrapperRealSockets2 : IntegrationTestsHelpers.BaseWrapperRealSockets
    {
        public IApiOperator Operator { get; }
        public Task MessageDisplayed => _remoteTraceMonitorConsistentTester.MessageDisplayed;

        private readonly RemoteTraceMonitorConsistentTester _remoteTraceMonitorConsistentTester;
            
        public RemoteOperatorTestWrapperRealSockets2(IReadOnlyList<SocketSubstitution> sockets, IApiOperator @operator, ApplicationCacheRecorder recorder, RemoteTraceMonitorConsistentTester remoteTraceMonitorConsistentTester) 
            : base(sockets, recorder)
        {
            Operator = @operator;
            _remoteTraceMonitorConsistentTester = remoteTraceMonitorConsistentTester;
        }

        public Task<bool> RaiseCommandReceived(string value)
        {
            return _remoteTraceMonitorConsistentTester.RaiseCommandReceived(value);
        }
    }
    
    internal class TraceableRemoteApiMapWrapperRealSockets2 : IntegrationTestsHelpers.BaseWrapperRealSockets
    {
        public ITraceableRemoteApiMap ApiMap { get; }

        public TraceableRemoteApiMapWrapperRealSockets2(IReadOnlyList<SocketSubstitution> sockets, ITraceableRemoteApiMap apiMap, ApplicationCacheRecorder recorder)
            : base(sockets, recorder)
        {
            ApiMap = apiMap;
        }
    }
    
    internal class RemoteOperatorTestWrapperRealSockets
    {
        public IReadOnlyList<SocketTesterWrapper> Sockets { get; }
        public IApiOperator Operator { get; }
        public ApplicationCacheRecorder Recorder { get; }
        public Task MessageDisplayed => _remoteTraceMonitorConsistentTester.MessageDisplayed;

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
    }
    
    internal class TraceableRemoteApiMapWrapperRealSockets
    {
        public IReadOnlyList<SocketTesterWrapper> Sockets { get; }
        public ITraceableRemoteApiMap ApiMap { get; }
        public ApplicationCacheRecorder Recorder { get; }
        public Task ConnectedComplete => _connectedTask.Task;

        private TaskCompletionSource<bool> _connectedTask;

        public Action<SocketTesterWrapper> SocketAcceptedHandler { get; }

        public TraceableRemoteApiMapWrapperRealSockets(IReadOnlyList<SocketTesterWrapper> sockets, ITraceableRemoteApiMap apiMap, ApplicationCacheRecorder recorder)
        {
            Sockets = sockets;
            ApiMap = apiMap;
            Recorder = recorder;
            _connectedTask = new TaskCompletionSource<bool>();

            SocketAcceptedHandler = SocketAccepted;
        }

        private async void SocketAccepted(SocketTesterWrapper socket)
        {
            _connectedTask = new TaskCompletionSource<bool>();
            
            await socket.ReceivedCalledSecondTime;
            
            _connectedTask.SetResult(true);
        }
    }

}