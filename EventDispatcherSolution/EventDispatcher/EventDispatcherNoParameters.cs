using System;
using System.Collections.Generic;

namespace ID5D6AAC.Common.EventDispatcher
{
    internal class EventDispatcherNoParameters
    {
        private Dictionary<string, HashSet<Action>> _subscriptionsNoParameters;

        internal EventDispatcherNoParameters()
        {
            _subscriptionsNoParameters = new Dictionary<string, HashSet<Action>>();
        }

        internal void AddEventListener(string eventType, Action handler)
        {
            if (!_subscriptionsNoParameters.ContainsKey(eventType))
            {
                _subscriptionsNoParameters[eventType] = new HashSet<Action>();
            }

            _subscriptionsNoParameters[eventType].Add(handler);
        }
        
        internal void DispatchEvent(string eventType)
        {
            if (_subscriptionsNoParameters.ContainsKey(eventType))
            {
                foreach( var action in _subscriptionsNoParameters[eventType] ) { action.Invoke(); }
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
    }
}