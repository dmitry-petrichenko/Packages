using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IConsoleAbstraction _consoleAbstraction;
        private readonly IСonsistentMessageSender _consistentMessageSender;
        private readonly IRecorder _recorder;

        private bool IsLocalActivated { get; set; }
        private bool IsRemoteActivated { get; set; }

        public MessageStreamer(
            IRecorderStream recorderStream, 
            IConsoleAbstraction consoleAbstraction,
            IСonsistentMessageSender consistentMessageSender)
        {
            _recorderStream = recorderStream;
            _consoleAbstraction = consoleAbstraction;
            _consistentMessageSender = consistentMessageSender;

            _consistentMessageSender.SendMessage += SendRemoteMessageHandler;
            _recorderStream.MessageReceived += MessageReceivedHandler;
        }

        private Task<(bool, IEnumerable<byte>)> SendRemoteMessageHandler(IEnumerable<byte> value)
        {
            return SendInstruction?.Invoke(value);
        }

        private void MessageReceivedHandler(string message)
        {
            if (IsRemoteActivated)
            {
                _consistentMessageSender.SendRemote(message);
            }

            if (IsLocalActivated)
            {
                SendLocal(message);
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
            _consoleAbstraction.WriteLine(message);
        }
    }
}