using System;
using C8F2740A.Common.Records;

namespace RemoteApi.Integration.Helpers
{
    public class CacheRecorder : IRecorder
    {
        public string InfoCache { get; private set; }
        public string ErrorCache { get; private set; }
        public int RecordErrorCalledTimes { get; private set; }

        public CacheRecorder()
        {
            InfoCache = string.Empty;
            ErrorCache = string.Empty;
        }
        
        public void RecordInfo(string tag, string message)
        {
            InfoCache += $"{tag}:{message}{Environment.NewLine}";
        }

        public void RecordError(string tag, string message)
        {
            RecordErrorCalledTimes++;
            ErrorCache += $"{tag}:{message}{Environment.NewLine}";
        }

        public void DefaultException(object source, Exception exception)
        {
            RecordError(source.GetType().Name, exception.Message);
        }
    }
}