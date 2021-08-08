using System;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public class SystemRecorder : ISystemRecorder
    {
        public event Action<string> InfoMessageReceived;
        public event Action<string> InterruptedWithMessage;
        
        public void InterruptWithMessage(string message)
        {
            InterruptedWithMessage?.Invoke(message);
        }
        
        public void RecordInfo(string message)
        {
            InfoMessageReceived?.Invoke(message);
        }
    }
}