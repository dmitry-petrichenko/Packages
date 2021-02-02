using System;
using System.Collections.Generic;
using System.Text;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.RemoteApi.Trace
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

        public void ActivateAndSendCache()
        {
            SafeExecution.TryCatch(() => ActivateAndSendCacheInternal(),
                exception => _recorder.DefaultException(this, exception));
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
        
        private void ActivateAndSendCacheInternal()
        {
            Activated = true;
            var cache = _applicationRecorder.GetCache();
            _consistentMessageSender.SendRemote(CacheToString(cache));
        }

        private string CacheToString(IEnumerable<string> cache)
        {
            var result = new StringBuilder();
            foreach (var s in cache)
            {
                result.Append($"{s}{Environment.NewLine}");
            }

            return result.ToString();
        }
    }
}