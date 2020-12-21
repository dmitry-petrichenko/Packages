using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace RemoteApi.Trace
{
    public interface IMessageStreamer
    {
        void SetLocalStreaming(bool value);
        void SetRemoteStreaming(bool value);
        IEnumerable<string> GetCache();
        
        event Func<IEnumerable<byte>, Task<(bool, IEnumerable<byte>)>> SendInstruction;
    }

    public class MessageStreamer : IMessageStreamer
    {
        private readonly IRecorderStream _recorderStream;
        private readonly IRecorder _recorder;

        private bool IsLocalActivated { get; set; }
        private bool IsRemoteActivated { get; set; }

        public MessageStreamer(IRecorderStream recorderStream, IRecorder recorder)
        {
            _recorderStream = recorderStream;
            _recorder = recorder;
            
            _recorderStream.MessageReceived += MessageReceivedHandler;
        }
        
        private void MessageReceivedHandler(string message)
        {
            SafeExecution.TryCatchAsync(MessageReceivedHandlerInternal(message),
                exception => _recorder.DefaultException(this, exception));
        }

        private async Task MessageReceivedHandlerInternal(string message)
        {
            if (IsRemoteActivated)
            {
                SendRemote(message);
            }

            if (IsLocalActivated)
            {
                SendLocal(message);
            }
        }

        private async Task SendRemote(string message)
        {
            var result = await SendInstruction?.Invoke(message.ToEnumerableByte());

            if (!result.Item1)
            {
                throw new Exception("Fail to send message");
            }
        }

        public void SetLocalStreaming(bool value)
        {
            IsLocalActivated = value;
        }

        public void SetRemoteStreaming(bool value)
        {
            IsRemoteActivated = value;
        }

        public IEnumerable<string> GetCache()
        {
            return _recorderStream.GetCache();
        }

        public event Func<IEnumerable<byte>, Task<(bool, IEnumerable<byte>)>> SendInstruction;

        private void SendLocal(string message)
        {
            Console.WriteLine(message);
        }
    }
}