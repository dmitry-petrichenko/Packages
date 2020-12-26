using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IRemoteRecordsSender
    {
        void ActivateAndSendCache();
    }
    
    public class RemoteRecordsSender : IRemoteRecordsSender
    {
        private bool Activated { get; set; }

        private readonly IСonsistentMessageSender _consistentMessageSender;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly IRecorder _recorder;
        
        public RemoteRecordsSender(
            IСonsistentMessageSender consistentMessageSender,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _consistentMessageSender = consistentMessageSender;
            _applicationRecorder = applicationRecorder;
            _recorder = recorder;

            _applicationRecorder.RecordReceived += RecordReceivedHandler;
        }

        private void RecordReceivedHandler(string value)
        {
            if (Activated)
            {
                SafeExecution.TryCatch(() => TrySendText(value),
                    exception => _recorder.DefaultException(this, exception));
            }
        }

        private void TrySendText(string value)
        {
            _consistentMessageSender.SendRemote(value);
        }

        public void ActivateAndSendCache()
        {
            Activated = true;
        }
    }
}