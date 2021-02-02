using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace RemoteApi.Integration.Helpers
{
    public class ApplicationCacheRecorder : IApplicationRecorder
    {
        public string AppInfoCache { get; private set; }
        public string AppErrorCache { get; private set; }
        public string SystemInfoCache { get; private set; }
        public string SystemErrorCache { get; private set; }
        public string DisplayMessagesCache { get; private set; }
        public int SystemErrorCalledTimes { get; private set; }
        public int AppErrorCalledTimes { get; private set; }
        public int AppInfoCalledTimes { get; private set; }
        public int DisplayMessagesCalledTimes { get; private set; }
        
        private IMessagesCache _messageCache;
        
        public ApplicationCacheRecorder()
        {
            AppInfoCache = string.Empty;
            AppErrorCache = string.Empty;
            SystemInfoCache = string.Empty;
            SystemErrorCache = string.Empty;
            DisplayMessagesCache = string.Empty;

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
            var formatted = $"{tag}:{message}{Environment.NewLine}";
            _messageCache.AddMessage(formatted);
            AppInfoCache += formatted;
            RecordReceived?.Invoke(formatted);     
        }

        void IApplicationRecorder.RecordError(string tag, string message)
        {
            AppErrorCalledTimes++;
            var formatted = $"{tag}:{message}{Environment.NewLine}";
            _messageCache.AddMessage(formatted);
            AppErrorCache += formatted;
            RecordReceived?.Invoke(formatted);     
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
            DisplayMessagesCache = string.Empty;
            SystemErrorCalledTimes = 0;
            AppErrorCalledTimes = 0;
            AppInfoCalledTimes = 0;
            DisplayMessagesCalledTimes = 0;
        }

        public void DisplayNextMessage(string message)
        {
            DisplayMessagesCalledTimes++;
            if (message.Equals(string.Empty))
            {
                DisplayMessagesCache += "empty\n\r";
            }
            else
            {
                DisplayMessagesCache += message;
            }
        }
        
        public void ClearTextBox()
        {
            DisplayMessagesCache += "-\n\r";
        }
    }
}