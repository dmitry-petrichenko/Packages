using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface IInstructionsReceiver
    {
        event Func<IEnumerable<byte>, IEnumerable<byte>> CommandReceived;
    }
    
    public class InstructionsReceiver : IInstructionsReceiver
    {
        private readonly INodeGatewayFactory _nodeGatewayFactory;
        private readonly INetworkAddress _localAddress;
        private readonly IRecorder _recorder;
        private readonly INodeGateway _nodeGateway;

        public InstructionsReceiver(INodeGatewayFactory nodeGatewayFactory, INetworkAddress localAddress, IRecorder recorder)
        {
            _localAddress = localAddress;
            _nodeGatewayFactory = nodeGatewayFactory;
            _recorder = recorder;

            _nodeGateway = _nodeGatewayFactory.Create($"{_localAddress.IP}:{_localAddress.Port}");
            _nodeGateway.CommandReceived += CommandReceivedHandler;
        }

        private void CommandReceivedHandler(byte[] bytes)
        {
            SafeExecution.TryCatch(() => CommandReceivedHandlerInternal(bytes), ExceptionHandler);
        }

        private void CommandReceivedHandlerInternal(byte[] bytes)
        {
            var result = CommandReceived?.Invoke(bytes);
            _nodeGateway.SendCommand(result.ToArray());
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(InstructionsReceiver), exception.Message);
        }

        public event Func<IEnumerable<byte>, IEnumerable<byte>> CommandReceived;
    }
}