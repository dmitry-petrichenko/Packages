using System;

namespace C8F2740A.Common.Records
{
    public class DefaultRecorder : IRecorder
    {
        public bool ShowErrors { get; set; }
        public bool ShowInfo { get; set; }
        
        public DefaultRecorder()
        {
        }

        public string GetHexCodeByHesh(int heshCode)
        {
            return heshCode.ToString("X");
        }

        public virtual void RecordInfo(string tag, string message)
        {
            if (ShowInfo)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{tag}: {message}");
                Console.ResetColor();
            }
        }

        public virtual void RecordError(string tag, string message)
        {
            if (ShowErrors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{tag}: {message}");
                Console.ResetColor();
            }
        }
        
        public void DefaultException(object source, Exception exception)
        {
            RecordError(source.GetType().Name, exception.Message);
        }
    }
}