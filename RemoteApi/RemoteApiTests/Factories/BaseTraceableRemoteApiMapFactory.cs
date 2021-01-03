using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi.Trace;

namespace RemoteApi.Factories
{
    public interface ITraceableRemoteApiMapFactory
    {
        ITraceableRemoteApiMap Create(string address);
    }

    public class BaseTraceableRemoteApiMapFactory : ITraceableRemoteApiMapFactory
    {
        private readonly IApplicationRecorder _recorder;
        private readonly IInstructionReceiverFactory _instructionReceiverFactory;

        public BaseTraceableRemoteApiMapFactory(
            IInstructionReceiverFactory instructionReceiverFactory,
            IApplicationRecorder recorder)
        {
            _instructionReceiverFactory = instructionReceiverFactory;
            _recorder = recorder;
        }

        public ITraceableRemoteApiMap Create(string address)
        {
            var instructionReceiver = _instructionReceiverFactory.Create(address);
            var remoteApiMap = new RemoteApiMap(instructionReceiver, _recorder);
            var consistentMessageSender = new СonsistentMessageSender(remoteApiMap, _recorder);
            var remoteRecorderSender = new RemoteRecordsSender(consistentMessageSender, _recorder, _recorder);
            var traceableRemoteApiMap = new TraceableRemoteApiMap(remoteApiMap, remoteRecorderSender, _recorder);

            return traceableRemoteApiMap;
        }
    }
}