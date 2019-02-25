using System;

namespace ID5D6AAC.Common.EventDispatcher
{
    public interface IEventDispatcher : IDisposable
    {
        void AddEventListener(string eventType, Action handler);
        void AddEventListener<TData>(string eventType, Action<TData> handler);
        void DispatchEvent(string eventType);
        void DispatchEvent<TData>(string eventType, TData data);
        void RemoveEventListener(string eventType, Delegate action);
    }
}