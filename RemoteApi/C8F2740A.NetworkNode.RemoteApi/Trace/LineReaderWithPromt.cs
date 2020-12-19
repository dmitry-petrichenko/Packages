using System;
using System.Threading.Tasks;

namespace RemoteApi.Trace
{
    public interface ILineReaderWithPrompt
    {
        void SetPrompt(string value);
        Task<string> ReadLineAsync();

        string Prompt { get; }
    }
    
    public class LineReaderWithPrompt : ILineReaderWithPrompt
    {
        private readonly IConsoleAbstraction _consoleAbstraction;
        private readonly IAsyncLineReader _asyncLineReader;
        private readonly int _height;
        
        public LineReaderWithPrompt(
            IAsyncLineReader asyncLineReader, 
            IConsoleAbstraction consoleAbstraction,
            int height)
        {
            _consoleAbstraction = consoleAbstraction;
            _asyncLineReader = asyncLineReader;
            _height = height;
            SetPrompt("127.0.0.1:0");
        }

        public void SetPrompt(string value)
        {
            _consoleAbstraction.ClearLine(0, _height);
            Prompt = value + " ";
        }

        public Task<string> ReadLineAsync()
        {
            _consoleAbstraction.WriteOnPosition(Prompt, 0, _height, ConsoleColor.Green);
            return _asyncLineReader.ReadLineOnPositionAsync(Prompt.Length, _height);
        }

        public string Prompt { get; private set; }
    }
}