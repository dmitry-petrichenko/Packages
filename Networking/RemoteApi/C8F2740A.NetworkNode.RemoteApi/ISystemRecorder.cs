using System;

namespace C8F2740A.NetworkNode.RemoteApi
{
    public interface ISystemRecorder : ISystemInterrupter,
                                       ISystemMessageDispatcher
    {
        void RecordInfo(string message);
    }
    
    public interface ISystemInterrupter
    {
        void InterruptWithMessage(string message);
    }
    
    public interface ISystemMessageDispatcher : ISystemInfoMessageDispatcher
    {
        event Action<string> InterruptedWithMessage;
    }
    
    public interface ISystemInfoMessageDispatcher
    {
        event Action<string> InfoMessageReceived;
    }
}