using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;

namespace RemoteApi.Integration2.Helpers
{
    public class ApplicationCacheRecorder : IApplicationRecorder, ISystemRecorder
    {
        public string AppInfoCache { get; private set; }
        public string AppErrorCache { get; private set; }
        public string SystemInfoCache { get; private set; }
        public string SystemErrorCache { get; private set; }
        public string InterruptMessagesCache { get; private set; }
        public int SystemErrorCalledTimes { get; private set; }
        public int AppErrorCalledTimes { get; private set; }
        public int AppInfoCalledTimes { get; private set; }
        
        private IMessagesCache _messageCache;
        
        public ApplicationCacheRecorder()
        {
            AppInfoCache = string.Empty;
            AppErrorCache = string.Empty;
            SystemInfoCache = string.Empty;
            SystemErrorCache = string.Empty;
            InterruptMessagesCache = string.Empty;
            
            _messageCache = new MessagesCache(10);
        }

        public event Action<string> RecordReceived;
        
        public IEnumerable<string> GetCache()
        {
            return _messageCache.GetCache();
        }

        void IApplicationRecorder.RecordInfo(string tag, string message)
        {
            AppInfoCalledTimes++;
            _messageCache.AddMessage($"{tag}:{message}{Environment.NewLine}");
            AppInfoCache += $"{tag}:{message}{Environment.NewLine}";
        }

        void IApplicationRecorder.RecordError(string tag, string message)
        {
            AppErrorCalledTimes++;
            _messageCache.AddMessage($"{tag}:{message}{Environment.NewLine}");
            AppErrorCache += $"{tag}:{message}{Environment.NewLine}";
        }

        void IRecorder.RecordInfo(string tag, string message)
        {
            SystemInfoCache += $"{tag}:{message}{Environment.NewLine}";
        }

        void IRecorder.RecordError(string tag, string message)
        {
            SystemErrorCalledTimes++;
            SystemErrorCache += $"{tag}:{message}{Environment.NewLine}";
        }

        public void DefaultException(object source, Exception exception)
        {
            ((IRecorder)this).RecordError(source.GetType().Name, exception.Message);
        }
        
        public void ClearCache()
        {
            AppInfoCache = string.Empty;
            AppErrorCache = string.Empty;
            SystemInfoCache = string.Empty;
            SystemErrorCache = string.Empty;
            InterruptMessagesCache = string.Empty;
            SystemErrorCalledTimes = 0;
            AppErrorCalledTimes = 0;
            AppInfoCalledTimes = 0;
        }

        // SYSTEM RECORDER INTERFACE
        public void InterruptWithMessage(string message)
        {
            InterruptMessagesCache += $"{message}{Environment.NewLine}";
        }

        public event Action<string> InfoMessageReceived;
        public event Action<string> InterruptedWithMessage;
        public void RecordInfo(string message)
        {
        }
    }
}