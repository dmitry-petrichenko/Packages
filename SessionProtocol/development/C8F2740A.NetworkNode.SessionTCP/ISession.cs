using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface ISession : IDisposable
    {
        void Response(IEnumerable<byte> data);
        void Send(IEnumerable<byte> data);
        
        event Action<IEnumerable<byte>> Received;
        event Action<IEnumerable<byte>> Responded;
        event Action Closed;
    }

    public class Session : ISession
    {
        private readonly INetworkTunnel _networkTunnel;
        private readonly IRecorder _recorder;
        private readonly byte _requestBytePrefix, _responseBytePrefix;

        private const byte REQUEST = 0b1100_0000;
        private const byte RESPONSE = 0b0011_0000;

        private IndexerCalculator _requestCalculator, _responseCalculator;
        private Dictionary<byte, Action<IEnumerable<byte>>> _responceEventMap;
        private bool _innerCallIsSent;
        private bool _outerCallIsSent;
        
        public event Action<IEnumerable<byte>> Received;
        public event Action<IEnumerable<byte>> Responded;
        public event Action Closed;

        public Session(INetworkTunnel networkTunnel, IRecorder recorder)
        {
            _networkTunnel = networkTunnel;
            _recorder = recorder;
            _requestCalculator = new IndexerCalculator(false);
            _responseCalculator = new IndexerCalculator(true);

            _requestBytePrefix = REQUEST.ExtractBytePrefix();
            _responseBytePrefix = RESPONSE.ExtractBytePrefix();

            _responceEventMap = new Dictionary<byte, Action<IEnumerable<byte>>>
            {
                { _requestBytePrefix, BytesReceivedHandler },
                { _responseBytePrefix, BytesRespondedHandler },
                { 0, BytesUnknownHandler },
            };

            _networkTunnel.Listen();
            _networkTunnel.Received += ReceivedHandler;
            _networkTunnel.Closed += CloseHandler;
        }

        public void Dispose()
        {
            _networkTunnel.Received -= ReceivedHandler;
            _networkTunnel.Closed -= CloseHandler;
        }

        public void Response(IEnumerable<byte> data)
        {
            SafeExecution.TryCatch(() => ResponseInternal(data), ExceptionHandler);
        }

        public void Send(IEnumerable<byte> data)
        {
            SafeExecution.TryCatch(() => SendInternal(data), ExceptionHandler);
        }

        private void ReceivedHandler(byte[] receivedBytes)
        {
            SafeExecution.TryCatch(() => ReceivedHandlerInternal(receivedBytes), ExceptionHandler);
        }

        // RESPONSE (INNER CALL)
        private void ResponseInternal(IEnumerable<byte> data)
        {
            if (_innerCallIsSent)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to send response without inner call");
                return;
            }
            
            _innerCallIsSent = true;
            
            var nextPrefix = _responseCalculator.GenerateIndexToSend(RESPONSE);
            data.WrapDataWithFirstByte(nextPrefix);
            
            _networkTunnel?.Send(data.ToArray());
        }

        private void ReceivedHandlerInternal(byte[] receivedBytes)
        {
            var receivedPrefix = receivedBytes.ExtractDataPrefix();
            var usefulData = receivedBytes.Skip(1).ToArray();
            _responceEventMap[ResolveDataType(receivedPrefix)](usefulData);
        }

        private byte ResolveDataType(byte firstByte)
        {
            if (firstByte.Equals(_requestBytePrefix))
            {
                return _requestBytePrefix;
            }
            
            if (firstByte.Equals(_responseBytePrefix))
            {
                return _responseBytePrefix;
            }

            return 0;
        }
        
        // REQUEST (INNER CALL)
        private void BytesReceivedHandler(IEnumerable<byte> data)
        {
            if (!_innerCallIsSent)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to receive request without inner call");
                return;
            }

            _innerCallIsSent = false;
            
            Received?.Invoke(data);
        }
        
        // RESPONCE (OUTER CALL)
        private void BytesRespondedHandler(IEnumerable<byte> data)
        {
            if (!_outerCallIsSent)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to receive responce without outer call");
                return;
            }

            _outerCallIsSent = false;
            
            Responded?.Invoke(data);
        }
        
        private void BytesUnknownHandler(IEnumerable<byte> data)
        {
            _recorder.RecordError(this.GetType().Name, "Wrong received data");
        }
        
        // REQUEST (OUTER CALL)
        private void SendInternal(IEnumerable<byte> data)
        {
            if (_outerCallIsSent)
            {
                _recorder.RecordError(this.GetType().Name, "");
                return;
            }
            
            _outerCallIsSent = true;
            
            var nextPrefix = _requestCalculator.GenerateIndexToSend(REQUEST);
            data.WrapDataWithFirstByte(nextPrefix);
            
            _networkTunnel?.Send(data.ToArray());
        }
        
        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(this.GetType().Name, exception.Message);
        }

        private void CloseHandler() => Closed?.Invoke();
    }
}