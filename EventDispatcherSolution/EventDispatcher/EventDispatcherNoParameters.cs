using System;
using System.Collections.Generic;

namespace ID5D6AAC.Common.EventDispatcher
{
    internal class EventDispatcherNoParameters : IDisposable
    {
        private Dictionary<string, List<Action>> _subscriptionsNoParameters;

        internal EventDispatcherNoParameters()
        {
            _subscriptionsNoParameters = new Dictionary<string, List<Action>>();
        }

        internal void AddEventListener(string eventType, Action handler)
        {
            if (!_subscriptionsNoParameters.ContainsKey(eventType))
            {
                _subscriptionsNoParameters[eventType] = new List<Action>();
            }

            _subscriptionsNoParameters[eventType].Add(handler);
        }
        
        internal void DispatchEvent(string eventType)
        {
            if (_subscriptionsNoParameters.ContainsKey(eventType))
            {
                List<Action> actions = _subscriptionsNoParameters[eventType];
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].Invoke();
                }
            }
        }
        
        internal void RemoveEventListener(string eventType, Action handler)
        {
            if (_subscriptionsNoParameters.ContainsKey(eventType))
            {
                if (_subscriptionsNoParameters[eventType].Contains(handler))
                {
                    _subscriptionsNoParameters[eventType].Remove(handler);
                }
            }
        }

        public void Dispose()
        {
            _subscriptionsNoParameters.Clear();
            _subscriptionsNoParameters = null;
        }
    }
}