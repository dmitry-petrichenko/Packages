using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;

namespace RemoteApi
{
    public interface IApplicationRecorder : IRecorder
    {
        event Action<string> RecordReceived;

        IEnumerable<string> GetCache();
        void RecordInfo(string tag, string message);
        void RecordError(string tag, string message);
    }
    
    public class ApplicationRecorder : IApplicationRecorder
    {
        private ISystemRecorder _systemRecorder;
        private IMessagesCache _messagesCache;
        
        public ApplicationRecorder(
            ISystemRecorder systemRecorder,
            IMessagesCache messagesCache)
        {
            _systemRecorder = systemRecorder;
            _messagesCache = messagesCache;
        }

        public event Action<string> RecordReceived;
        
        public void DefaultException(object source, Exception exception)
        {
            _systemRecorder.InterruptWithMessage(FormatErrorRecord(source.GetType().Name, exception.Message));
        }

        // --- IApplicationRecorder ---------------------------------------------//
        
        public IEnumerable<string> GetCache()
        {
            return _messagesCache.GetCache();
        }

        void IApplicationRecorder.RecordInfo(string tag, string message)
        {
            _messagesCache.AddMessage(FormatInfoRecord(tag, message));
            RecordReceived?.Invoke(FormatInfoRecord(tag, message));
        }
        
        void IApplicationRecorder.RecordError(string tag, string message)
        {
            _messagesCache.AddMessage(FormatInfoRecord(tag, message));
            RecordReceived?.Invoke(FormatErrorRecord(tag, message));
        }

        // --- IApplicationRecorder ---------------------------------------------//

        void IRecorder.RecordError(string tag, string message)
        {
            _systemRecorder.InterruptWithMessage(FormatErrorRecord(tag, message));
        }
        
        void IRecorder.RecordInfo(string tag, string message)
        {
            _systemRecorder.RecordInfo(FormatInfoRecord(tag, message));
        }
        
        private string FormatInfoRecord(string tag, string message)
        {
            return $"{DateTime.Now.ToShortTimeFormat()} {tag}:{message}";
        }
        
        private string FormatErrorRecord(string tag, string message)
        {
            return $"(e){tag}:{message}";
        }
    }
    
    public interface IMessagesCache
    {
        void AddMessage(string value);
        IEnumerable<string> GetCache();
    }
    
    public class MessagesCache : IMessagesCache
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