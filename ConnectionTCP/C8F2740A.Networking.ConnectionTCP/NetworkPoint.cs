﻿using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    public interface INetworkPoint : IDisposable
    {
        event Action<INetworkTunnel> Accepted;
    }
    
    public class NetworkPoint : INetworkPoint
    {
        private readonly ISocket _sListener;
        private readonly Func<ISocket, INetworkTunnel> _networkTunnelFactory;
        private readonly IRecorder _recorder;

        private bool _isOpened;

        public event Action<INetworkTunnel> Accepted;
        
        public NetworkPoint(
            INetworkAddress networkAddress, 
            Func<ISocket, INetworkTunnel> networkTunnelFactory,
            ISocketFactory socketFactory,
            IRecorder recorder)
        {
            _recorder = recorder;
            _networkTunnelFactory = networkTunnelFactory;
            _sListener = socketFactory.Create(networkAddress.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sListener.Bind(networkAddress.IP, networkAddress.Port);
            Open();
        }
        
        private void Open()
        {
            SafeExecution.TryCatchAsync(OpenInternal(), exception =>
            {
                _recorder.RecordError(nameof(NetworkPoint), exception.Message);
            });
        }

        public void Dispose()
        {
            Close();
            _sListener.Dispose();
        }

        private async Task OpenInternal()
        {
            _isOpened = true;
            _recorder.RecordInfo(nameof(NetworkPoint), $"NetworkPoint opened {(_sListener.LocalEndPoint).Address}:{(_sListener.LocalEndPoint).Port}");
            
            while (_isOpened)
            {
                _sListener.Listen(10);
                ISocket socket = await _sListener.AcceptAsync();

                ConnectionAcceptedHandler(socket);
            }
        }

        private void Close()
        {
            _isOpened = false;
        }

        private void ConnectionAcceptedHandler(ISocket socket)
        {
            var networkTunnel = _networkTunnelFactory.Invoke(socket);
            Accepted?.Invoke(networkTunnel);
        }
    }
}