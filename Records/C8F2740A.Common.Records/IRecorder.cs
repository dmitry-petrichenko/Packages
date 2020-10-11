using System;

namespace C8F2740A.Common.Records
{
    public interface IRecorder
    {
        string GetHexCodeByHesh(int heshCode);
        void RecordInfo(string tag, string message);
        void RecordError(string tag, string message);

        bool ShowErrors { set; }
        bool ShowInfo { set; }
    }
}