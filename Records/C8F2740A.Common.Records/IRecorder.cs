using System;

namespace C8F2740A.Common.Records
{
    public interface IRecorder
    {
        void RecordInfo(string tag, string message);
        void RecordError(string tag, string message);

        bool ShowErrors { set; }
        bool ShowInfo { set; }

        void DefaultException(object source, Exception exception);
    }
}