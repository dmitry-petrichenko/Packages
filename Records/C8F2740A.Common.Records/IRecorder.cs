using System;

namespace C8F2740A.Common.Records
{
    public interface IRecorder
    {
        string GetHexCodeByHesh(int heshCode);
        void RecordInfo(string tag, string message);
        void RecordError(string tag, string message);
    }
    
    public class DefaultRecorder : IRecorder
    {
        public string GetHexCodeByHesh(int heshCode)
        {
            return heshCode.ToString("X");
        }

        public void RecordInfo(string tag, string message)
        {
            Console.WriteLine($"{tag}: {message}");
        }

        public void RecordError(string tag, string message)
        {
            Console.WriteLine($"{tag}: {message}");
        }
    }
}