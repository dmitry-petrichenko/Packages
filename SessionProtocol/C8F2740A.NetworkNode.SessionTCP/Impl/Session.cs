using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public class Session : ISession
    {
        private readonly INetworkTunnel _networkTunnel;
        private readonly IRecorder _recorder;
        private readonly byte _requestBytePrefix, _responseBytePrefix;
        private readonly IndexerCalculator _requestCalculator, _responseCalculator;
        private readonly Dictionary<byte, Action<IEnumerable<byte>, byte>> _responceEventMap;

        internal static readonly byte REQUEST = 0b1100_0000;
        internal static readonly byte RESPONSE = 0b0011_0000;
        
        private bool _requestFromRemoteReceived;
        private bool _requestToRemoteSent;
        
        public event Action<IEnumerable<byte>> Received;
        public event Action<IEnumerable<byte>> Responded;
        public event Action Disconnected;

        public Session(INetworkTunnel networkTunnel, IRecorder recorder)
        {
            _networkTunnel = networkTunnel;
            _recorder = recorder;
            _requestCalculator = new IndexerCalculator(false);
            _responseCalculator = new IndexerCalculator(true);

            _requestBytePrefix = REQUEST.ExtractBytePrefix();
            _responseBytePrefix = RESPONSE.ExtractBytePrefix();

            _responceEventMap = new Dictionary<byte, Action<IEnumerable<byte>, byte>>
            {
                { _requestBytePrefix, BytesReceivedHandler },
                { _responseBytePrefix, BytesRespondedHandler },
                { 0, BytesUnknownHandler },
            };
            
            _networkTunnel.Received += ReceivedHandler;
            _networkTunnel.Disconnected += DisconnectHandler;

            _requestFromRemoteReceived = false;
            _requestToRemoteSent = false;
        }

        public void Listen()
        {
            _networkTunnel.Listen();
        }

        public void Dispose()
        {
            _networkTunnel.Received -= ReceivedHandler;
            _networkTunnel.Disconnected -= DisconnectHandler;
            _networkTunnel.Dispose();
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

        private void ReceivedHandlerInternal(byte[] receivedBytes)
        {
            var receivedIndex = receivedBytes.ExtractDataIndex();
            var receivedPrefix = receivedBytes.ExtractDataPrefix();
            var usefulData = receivedBytes.Skip(1).ToArray();
            _responceEventMap[ResolveDataType(receivedPrefix)](usefulData, receivedIndex);
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
        
        // RESPONSE (INNER CALL)
        private void ResponseInternal(IEnumerable<byte> data)
        {
            if (!_requestFromRemoteReceived)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to send response without inner call");
                return;
            }
            
            _requestFromRemoteReceived = false;
            
            var nextPrefix = _responseCalculator.GenerateIndexToSend(RESPONSE);
            var dataWithPrefix = data.WrapDataWithFirstByte(nextPrefix);
            
            _networkTunnel?.Send(dataWithPrefix.ToArray());
        }
        
        // REQUEST (INNER CALL)
        private void BytesReceivedHandler(IEnumerable<byte> data, byte index)
        {
            if (_requestFromRemoteReceived)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to receive request without inner call");
                return;
            }

            if (!_responseCalculator.ValidateCurrentIndex(index))
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Received request with wrong index");
                return;
            }
            
            _requestFromRemoteReceived = true;
            
            Received?.Invoke(data);
        }
        
        // RESPONSE (OUTER CALL)
        private void BytesRespondedHandler(IEnumerable<byte> data, byte prefix)
        {
            if (!_requestToRemoteSent)
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Trying to receive response without outer call");
                return;
            }
            
            if (!_requestCalculator.ValidateCurrentIndex(prefix))
            {
                _recorder.RecordError(this.GetType().Name, 
                    "Received response with wrong index");
                return;
            }

            _requestToRemoteSent = false;
            
            Responded?.Invoke(data);
        }
        
        private void BytesUnknownHandler(IEnumerable<byte> data, byte prefix)
        {
            _recorder.RecordError(this.GetType().Name, "Wrong received data");
        }
        
        // REQUEST (OUTER CALL)
        private void SendInternal(IEnumerable<byte> data)
        {
            if (_requestToRemoteSent)
            {
                _recorder.RecordError(this.GetType().Name, "");
                return;
            }
            
            _requestToRemoteSent = true;
            
            var nextPrefix = _requestCalculator.GenerateIndexToSend(REQUEST);
            var dataWithPrefix = data.WrapDataWithFirstByte(nextPrefix);
            
            _networkTunnel?.Send(dataWithPrefix.ToArray());
        }
        
        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(GetType().Name, exception.Message);
        }

        private void DisconnectHandler()
        {
            Disconnected?.Invoke();
        }
    }
}