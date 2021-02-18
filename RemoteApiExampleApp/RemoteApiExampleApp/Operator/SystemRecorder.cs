using System;
using C8F2740A.NetworkNode.RemoteApi;

namespace Operator
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