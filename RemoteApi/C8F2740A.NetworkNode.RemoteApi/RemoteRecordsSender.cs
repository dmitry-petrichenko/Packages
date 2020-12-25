using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace RemoteApi
{
    public interface IRemoteRecordsSender
    {
        void ActivateAndSendCache();
    }
    
    public class RemoteRecordsSender : IRemoteRecordsSender
    {
        private bool Activated { get; set; }

        private readonly ITextToRemoteSender _textToRemoteSender;
        private readonly IApplicationRecorder _applicationRecorder;
        private readonly IRecorder _recorder;
        
        public RemoteRecordsSender(
            ITextToRemoteSender textToRemoteSender,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _textToRemoteSender = textToRemoteSender;
            _applicationRecorder = applicationRecorder;
            _recorder = recorder;

            _applicationRecorder.RecordReceived += RecordReceivedHandler;
        }

        private void RecordReceivedHandler(string value)
        {
            if (Activated)
            {
                SafeExecution.TryCatchAsync(TrySendText(value),
                    exception => _recorder.DefaultException(this, exception));
            }
        }

        private async Task TrySendText(string value)
        {
            var isSent = await _textToRemoteSender.TrySendText(value);

            if (!isSent)
            {
                _recorder.RecordError(GetType().Name, "Fail to send text");
            }
        }

        public void ActivateAndSendCache()
        {
            Activated = true;
        }
    }
}