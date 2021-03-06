﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    public interface INetworkTunnel
    {
        Task Listen();
        void Send(byte[] data);
        void Dispose();

        event Action<byte[]> Received;
        event Action Disconnected;
    }
    
    public class NetworkTunnel : INetworkTunnel
    {
        public event Action<byte[]> Received;
        public event Action Disconnected;
        
        private readonly ISocket _socket;
        private readonly IRecorder _recorder;

        public NetworkTunnel(ISocket socket, IRecorder recorder)
        {
            _recorder = recorder;
            _socket = socket;
            
            RecordOpenCloseInfo("Tunnel opened");
        }

        public void Send(byte[] data)
        {
            RecordSendInfo($"Tunnel.Send {data.Length}");
            SafeExecution.TryCatch(() => _socket.Send(data), ExceptionHandler);
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        public Task Listen()
        {
            return SafeExecution.TryCatchAsync(()  => Task.Run(ListenInternal), ExceptionHandler);
        }
        
        private void ListenInternal()
        {
            byte[] data = new byte[1024];

            while (_socket.Connected)
            {
                int bytes = _socket.Receive(data);
                RecordReceivedInfo($"Bytes received {bytes}");
                
                if (bytes == 0) 
                    break;
                
                Received?.Invoke(data.Take(bytes).ToArray());
            }

            Disconnected?.Invoke();
        }
        
        private void ExceptionHandler(Exception exception)
        {
            SocketException socketException = exception as SocketException;

            if (socketException == null)
            {
                RecordError(exception.Message);
                return;
            }

            if (socketException.ErrorCode == 10054)
            {
                Disconnected?.Invoke();
            }
            else
            {
                RecordError(socketException.Message);
            }
        }

        private void DisposeInternal()
        {
            RecordOpenCloseInfo("Tunnel closed");
            _socket.Close();
            _socket.Dispose();
        }

        private void RecordError(string message)
        {
            _recorder.RecordError(nameof(NetworkTunnel), BuildMessageError(message));
        }
        
        private void RecordReceivedInfo(string message)
        {
            IPEndPoint local = _socket.LocalEndPoint;
            IPEndPoint remote = _socket.RemoteEndPoint;
            _recorder.RecordInfo(GetType().Name, 
                $"{message}: ({remote.Address}:{remote.Port}) -> ({local.Address}:{local.Port})");
        }
        
        private void RecordSendInfo(string message)
        {
            IPEndPoint local = _socket.LocalEndPoint;
            IPEndPoint remote = _socket.RemoteEndPoint;
            _recorder.RecordInfo(GetType().Name, 
                $"{message}: ({local.Address}:{local.Port}) -> ({remote.Address}:{remote.Port})");
        }
        
        private void RecordOpenCloseInfo(string message)
        {
            if (!_socket.Connected)
            {
                _recorder.RecordInfo(GetType().Name, 
                    $"{message}: Closed socket");
                return;
            }
            
            IPEndPoint local = _socket.LocalEndPoint;
            _recorder.RecordInfo(GetType().Name, 
                $"{message}: local ({local.Address}:{local.Port})");
        }

        private string BuildMessageError(string message)
        {
            IPEndPoint ipEndPoint = _socket.LocalEndPoint;
            return $"{message}: local ({ipEndPoint.Address}:{ipEndPoint.Port})";
        }
    }
}