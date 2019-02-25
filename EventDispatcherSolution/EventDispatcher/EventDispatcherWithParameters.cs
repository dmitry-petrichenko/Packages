using System;
using System.Collections.Generic;

namespace ID5D6AAC.Common.EventDispatcher
{
    internal class EventDispatcherWithParameters : IDisposable
    {
        private Dictionary<string, Dictionary<Delegate, IEventhandlerWithParameter>> _subscriptionsWithParameters;
        
        internal EventDispatcherWithParameters()
        {
            _subscriptionsWithParameters = new Dictionary<string, Dictionary<Delegate, IEventhandlerWithParameter>>();
        }
        
        internal void AddEventListener<TData>(string eventType, Action<TData> handler)
        {
            if (!_subscriptionsWithParameters.ContainsKey(eventType))
            {
                _subscriptionsWithParameters[eventType] = new Dictionary<Delegate, IEventhandlerWithParameter>();
            }

            EventhandlerWithParameter<TData> eventhandler = new EventhandlerWithParameter<TData>(handler);
            _subscriptionsWithParameters[eventType][handler] = eventhandler;
        }
        
        internal void DispatchEvent<TData>(string eventType, TData data)
        {
            EventhandlerWithParameter<TData> eventhandlerWithParameter;
            if (_subscriptionsWithParameters.ContainsKey(eventType))
            {
                foreach(KeyValuePair<Delegate, IEventhandlerWithParameter> item in _subscriptionsWithParameters[eventType])
                {
                    eventhandlerWithParameter = item.Value as EventhandlerWithParameter<TData>;
                    eventhandlerWithParameter.Data = data;
                    eventhandlerWithParameter.Invoke();
                }
            }
        }
        
        internal void RemoveEventListener(string eventType, Delegate handler)
        {
            if (_subscriptionsWithParameters.ContainsKey(eventType))
            {
                if (_subscriptionsWithParameters[eventType].ContainsKey(handler))
                {
                    _subscriptionsWithParameters[eventType].Remove(handler);
                }
            }
        }
        
        private class EventhandlerWithParameter<TData> : IEventhandlerWithParameter
        {
            private Action<TData> _handler;
            private TData _data;

            public TData Data
            {
                set => _data = value;
            }

            public EventhandlerWithParameter(Action<TData> handler)
            {
                _handler = handler;
            }

            public void Invoke()
            {
                _handler.Invoke(_data);
            }
        }
        
        private interface IEventhandlerWithParameter
        {
            void Invoke();
        }

        public void Dispose()
        {
            _subscriptionsWithParameters.Clear();
            _subscriptionsWithParameters = null;
        }
    }
    
}