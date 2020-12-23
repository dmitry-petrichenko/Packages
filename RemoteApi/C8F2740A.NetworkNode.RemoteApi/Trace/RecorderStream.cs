using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;

namespace RemoteApi.Trace
{
    public interface IRecorderStream
    {
        IEnumerable<string> GetCache();
        event Action<string> MessageReceived;
    }
    
    public class RecorderStream : IRecorderStream, IRecorder
    {
        private readonly MessagesCache _messagesCache;
        
        public RecorderStream(int messageCache)
        {
            _messagesCache = new MessagesCache(messageCache);
        }

        public IEnumerable<string> GetCache()
        {
            return _messagesCache.GetCache();
        }

        public event Action<string> MessageReceived;
        
        private void WriteMessage(string tag, string value)
        {
            var message = $"{tag}:{value}";
            _messagesCache.AddMessage(message);
            MessageReceived?.Invoke(message);
        }

        public void RecordInfo(string tag, string message)
        {
            //WriteMessage($"(i){tag}", message);
        }

        public void RecordError(string tag, string message)
        {
            WriteMessage($"(e){tag}", message);
        }

        public void DefaultException(object source, Exception exception)
        {
            WriteMessage(source.GetType().Name, exception.Message);
        }

        private class MessagesCache
        {
            private readonly int _size;
            private Queue<string> _internalCache;
            
            public MessagesCache(int size)
            {
                _size = size;
                _internalCache = new Queue<string>(_size);
            }

            public void AddMessage(string value)
            {
                _internalCache.Enqueue(value);
                
                if (_internalCache.Count > _size)
                {
                    _internalCache.Dequeue();
                }
            }

            public IEnumerable<string> GetCache()
            {
                return _internalCache;
            }
        }
    }
}