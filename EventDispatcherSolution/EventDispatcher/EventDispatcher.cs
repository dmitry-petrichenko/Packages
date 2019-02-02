using System;

namespace ID5D6AAC.Common.EventDispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private EventDispatcherNoParameters _eventDispatcherNoParameters;
        private EventDispatcherWithParameters _eventDispatcherWithParameters;

        public EventDispatcher()
        {
            _eventDispatcherNoParameters = new EventDispatcherNoParameters();
            _eventDispatcherWithParameters = new EventDispatcherWithParameters();
        }

        public void AddEventListener(string eventType, Action handler)
        {
            _eventDispatcherNoParameters.AddEventListener(eventType, handler);
        }

        public void AddEventListener<TData>(string eventType, Action<TData> handler)
        {
            _eventDispatcherWithParameters.AddEventListener<TData>(eventType, handler);
        }

        public void DispatchEvent(string eventType)
        {
            _eventDispatcherNoParameters.DispatchEvent(eventType);
        }

        public void DispatchEvent<TData>(string eventType, TData data)
        {
            _eventDispatcherWithParameters.DispatchEvent(eventType, data);

        }

        public void RemoveEventListener(string eventType, Delegate handler)
        {
            _eventDispatcherWithParameters.RemoveEventListener(eventType, handler);
            Action action = handler as Action;
            _eventDispatcherNoParameters.RemoveEventListener(eventType, action);
        }
    }


}