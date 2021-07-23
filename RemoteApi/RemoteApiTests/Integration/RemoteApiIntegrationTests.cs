using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Operator;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi.Integration.Helpers;
using Telerik.JustMock;
using Xunit;
using Xunit.Abstractions;

namespace RemoteApi.Integration
{
    public partial class RemoteApiIntegrationTests
    {
        private readonly IRecorder _recorder;
        private readonly CacheRecorder _cacheRecorder;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly ISystemRecorder _systemRecorder;
        private readonly ITestOutputHelper _output;
        
        private readonly IMonitoredRemoteOperator _mro;
        private readonly ITraceableRemoteApiMap _tram;

        public RemoteApiIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
            _cacheRecorder = new CacheRecorder();
            _recorder = Mock.Create<IRecorder>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            _systemRecorder = Mock.Create<ISystemRecorder>();
        }

        private IApiOperator CreateLocalOperator(
            Func<AddressFamily, SocketType, ProtocolType, string, ISocket> socketFactory,
            IRecorder recorder,
            IRemoteTraceMonitorСonsistent remoteTraceMonitor = default,
            string address = "111.111.111.111:11111")
        {
            if (remoteTraceMonitor == default)
            {
                remoteTraceMonitor = Mock.Create<IRemoteTraceMonitorСonsistent>();
            }
            
            // MonitoredRemoteOperator
            var instructionSenderFactory = new TestInstructionSenderFactory(socketFactory, recorder);
            var monitoredRemoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(
                instructionSenderFactory, 
                remoteTraceMonitor, _applicationRecorder, recorder);

            // RemoteApiMap
            var instructionReceiverFactory = new TestInstructionReceiverFactory(socketFactory, recorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(
                instructionReceiverFactory,
                _applicationRecorder);
            
            var apiOperatorFactory = new ApiOperatorFactory(monitoredRemoteOperatorFactory, traceableRemoteApiMapFactory, _applicationRecorder);
            return apiOperatorFactory.Create(address);
        }
        
        private Func<AddressFamily, SocketType, ProtocolType, string, ISocket> ArrangeSocketFactoryLocal(
            SocketTester socketConnecter, 
            SocketTester socketListener, 
            SocketTester socketAccepted, 
            IEnumerable<SocketTester> otherSockets = default)
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
                socketAccepted.SetRemoteEndPoint(ip, port);
                socketAccepted.SetLocalEndPoint(socketConnecter.LocalEndPoint.Address, 53691);
                socketListener.RaiseSocketAccepted(socketAccepted);
            };
            
            ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, string tag)
            {
                return socketTesterFactory.Create();
            }
            
            return SocketFactory;
        }
    }
}