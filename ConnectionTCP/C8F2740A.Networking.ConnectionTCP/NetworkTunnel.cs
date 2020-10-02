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
    public interface INetworkTunnel : IDisposable
    {
        void Send(byte[] data);

        event Action<byte[]> Received;
        event Action Closed;
    }
    
    public class NetworkTunnel : INetworkTunnel
    {
        private readonly ISocket _socket;
        private readonly IRecorder _recorder;
        
        private INetworkAddress _networkAddress;
        private bool _isDisposed;
        
        public NetworkTunnel(ISocket socket, IRecorder recorder)
        {
            _recorder = recorder;
            _socket = socket;
            
            RecordInfo("Tunnel opened");
            Listen();
        }

        public void Send(byte[] data)
        {
            SafeExecution.TryCatch(() => _socket.Send(data), ExceptionHandler);
        }

        public event Action<byte[]> Received;
        public event Action Closed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            CloseInternal();
        }

        private async Task Listen()
        {
            SafeExecution.TryCatchAsync(Task.Run(ListenInternal), ExceptionHandler);
        }
        
        private void ListenInternal()
        {
            byte[] data = new byte[1024];

            while (_socket.Connected)
            {
                var bytes = _socket.Receive(data);
                
                if (bytes == 0) 
                    break;
                
                Received?.Invoke(data.Take(bytes).ToArray());
            }

            Dispose();
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
                CloseInternal();
            }
            else
            {
                RecordError(socketException.Message);
            }
        }

        private void CloseInternal()
        {
            RecordInfo("Tunnel closed");
            Closed?.Invoke();
            _socket.Close();
            _socket.Dispose();
        }

        private void RecordInfo(string message)
        {
            _recorder.RecordInfo(nameof(NetworkTunnel), BuildMessage(message));
        }
        
        private void RecordError(string message)
        {
            _recorder.RecordError(nameof(NetworkTunnel), BuildMessage(message));
        }

        private string BuildMessage(string message)
        {
            IPEndPoint ipEndPoint = _socket.RemoteEndPoint;
            return $"{message}: {ipEndPoint.Address}:{ipEndPoint.Port}";
        }
    }
}