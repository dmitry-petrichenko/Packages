using System;

namespace C8F2740A.Common.Records
{
    public class DefaultRecorder : IRecorder
    {
        private readonly IRecorderSettings _recorderSettings;
        
        public DefaultRecorder(IRecorderSettings recorderSettings)
        {
            _recorderSettings = recorderSettings;
        }

        public virtual void RecordInfo(string tag, string message)
        {
            if (_recorderSettings.ShowInfo)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{tag}: {message}");
                Console.ResetColor();
            }
        }

        public virtual void RecordError(string tag, string message)
        {
            if (_recorderSettings.ShowErrors)
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