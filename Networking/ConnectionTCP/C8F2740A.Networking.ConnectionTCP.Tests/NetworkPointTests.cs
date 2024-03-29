﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests
{
    public class NetworkPointTests
    {
        private INetworkPoint _sut;
        private INetworkAddress _networkAddress;
        private ISocketFactory _socketFactory;
        private Func<ISocket, INetworkTunnel> _networkTunnelFactory;
        private IRecorder _recorder;
        
        public NetworkPointTests()
        {
            _networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            _socketFactory = Mock.Create<ISocketFactory>();
            _recorder = Mock.Create<IRecorder>();
        }

        [Fact]
        public void ConstructorCall_WithParameters_ShouldStartListenSocket()
        {
            var socketMock = Mock.Create<ISegmentedSocket>();
            var socketFactory = ArrangeSocketFactory(socketMock);
            var networkTunnelFactory = ArrangeNetworkTunnelFactory();
            var networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            
            _sut = CreateNetworkPoint(networkAddress, networkTunnelFactory, socketFactory, _recorder);
            
            Mock.Assert(() => socketMock.Listen(Arg.AnyInt), Occurs.Exactly(1));
        }
        
        [Fact]
        public void ConstructorCall_WithParameters_ShouldCreateSocket()
        {
            var socketFactory = ArrangeSocketFactory();
            var networkTunnelFactory = ArrangeNetworkTunnelFactory();
            var networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            
            _sut = CreateNetworkPoint(networkAddress, networkTunnelFactory, socketFactory, _recorder);

            Mock.Assert(() => socketFactory.Invoke(Arg.IsAny<AddressFamily>(), Arg.IsAny<SocketType>(), Arg.IsAny<ProtocolType>(), Arg.IsAny<string>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void ConstructorCall_WithParameters_ShouldBindSocket()
        {
            var socketMock = Mock.Create<ISegmentedSocket>();
            var socketFactory = ArrangeSocketFactory(socketMock);
            var networkTunnelFactory = ArrangeNetworkTunnelFactory();
            var networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            
            _sut = CreateNetworkPoint(networkAddress, networkTunnelFactory, socketFactory, _recorder);
            
            Mock.Assert(() => socketMock.Bind(Arg.IsAny<IPAddress>(), Arg.AnyInt), Occurs.Exactly(1));
        }

        /*
        [Fact]
        public async Task AcceptAsync_Throws_ShouldBeCaught()
        {
            var socketMock = Mock.Create<ISocket>();
            Mock.Arrange(() => socketMock.AcceptAsync()).IgnoreArguments().Throws(new Exception("exception"));
            Mock.Arrange(() => socketMock.Close()).IgnoreArguments().Throws(new Exception("exception"));
            var socketFactory = ArrangeSocketFactory(socketMock, false);
            var networkTunnelFactory = ArrangeNetworkTunnelFactory();
            var networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            
            _sut = CreateNetworkPoint(networkAddress, networkTunnelFactory, socketFactory, _recorder);
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Exactly(1));
        }
        */

        private INetworkPoint CreateDefaultNetworkPoint()
        {
            var socketFactory = ArrangeSocketFactory();
            var networkTunnelFactory = ArrangeNetworkTunnelFactory();
            var networkAddress = ArrangeNetworkAddress(IPAddress.Any, 20);
            
            return CreateNetworkPoint(networkAddress, networkTunnelFactory, socketFactory, _recorder);
        }

        private Func<AddressFamily, SocketType, ProtocolType, string, ISegmentedSocket> ArrangeSocketFactory(ISegmentedSocket socketMock = default, bool returnTask = true)
        {
            if (socketMock == default)
            {
                socketMock = Mock.Create<ISegmentedSocket>();
            }
            
            if (returnTask)
            {
                var tcs = new TaskCompletionSource<ISegmentedSocket>();
                Mock.Arrange(() => socketMock.AcceptAsync()).Returns(tcs.Task);
            }

            var factory = Mock.Create<Func<AddressFamily, SocketType, ProtocolType, string, ISegmentedSocket>>();
            Mock.Arrange(() => factory.Invoke(default, default, default, default))
                .IgnoreArguments()
                .Returns(socketMock);

            return factory;
        }
        
        private Func<ISegmentedSocket, INetworkTunnel> ArrangeNetworkTunnelFactory(INetworkTunnel tunnelMock = default)
        {
            if (tunnelMock == default)
            {
                tunnelMock = Mock.Create<INetworkTunnel>();
            }

            var factory = new Func<ISegmentedSocket, INetworkTunnel>(s =>
            {
                return tunnelMock;
            });

            return factory;
        }
        
        private INetworkAddress ArrangeNetworkAddress(IPAddress ip, int port)
        {
            var address = Mock.Create<INetworkAddress>();
            Mock.Arrange(() => address.Port).Returns(port);
            Mock.Arrange(() => address.IP).Returns(ip);

            return address;
        }

        private INetworkPoint CreateNetworkPoint(
            INetworkAddress networkAddress, 
            Func<ISegmentedSocket, INetworkTunnel> networkTunnelFactory,
            Func<AddressFamily, SocketType, ProtocolType, string, ISegmentedSocket> socketFactory,
            IRecorder recorder)
        {
            var networkPoint = new NetworkPoint(networkAddress, networkTunnelFactory, socketFactory, recorder);
            return networkPoint;
        }
    }
}