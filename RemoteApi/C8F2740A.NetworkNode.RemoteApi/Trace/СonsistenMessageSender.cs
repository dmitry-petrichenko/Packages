using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace RemoteApi.Trace
{
    public interface IСonsistentMessageSender
    {
        void SendRemote(string message);
    }
    
    public class СonsistentMessageSender : IСonsistentMessageSender
    {
        private readonly ITextToRemoteSender _textToRemoteSender;
        private readonly IRecorder _recorder;
        
        private BlockingCollection<string> _messageQueue;
        private Task _sendingTask;

        public СonsistentMessageSender(ITextToRemoteSender textToRemoteSender, IRecorder recorder)
        {
            _messageQueue = new BlockingCollection<string>();
            _textToRemoteSender = textToRemoteSender;
            _recorder = recorder;

            StartProducing();
        }

        public void SendRemote(string message)
        {
            _messageQueue.Add(message);
        }
        
        private void StartProducing()
        {
            SafeExecution.TryCatchAsync(StartProducingInternal(),
                exception => _recorder.DefaultException(this, exception));
        }

        private async Task StartProducingInternal()
        {
            await Task.Run(async () =>
            {
                try
                {
                    foreach (var element in _messageQueue.GetConsumingEnumerable())
                    {
                        await ExecuteSendMessage(element);
                    }
                }
                catch (Exception e)
                {
                    _recorder.DefaultException(this, e);
                }
            });
        }

        private async Task ExecuteSendMessage(string message)
        {
            var isSent = await _textToRemoteSender.TrySendText(message);

            if (!isSent)
            {
                _recorder.RecordError(GetType().Name, "Message did not send");
            }
        }
    }
}