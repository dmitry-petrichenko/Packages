using System;
using System.Threading.Tasks;

namespace RemoteApi.Monitor
{
    public interface ILineReaderWithPrompt
    {
        void SetPrompt(string value);
        Task<string> ReadLineAsync();
        void SetCursorAfterPrompt();
        void Start();
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
        }

        public void SetPrompt(string value)
        {
            _consoleAbstraction.ClearLine(0, _height);
            Prompt = value + " ";
            DisplayPrompt();
        }

        public async Task<string> ReadLineAsync()
        {
            var result = string.Empty;
            try
            {
                result = await _asyncLineReader.ReadLineOnPositionAsync(() => Prompt.Length, _height);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        public void SetCursorAfterPrompt()
        {
            _consoleAbstraction.SetCursorPosition( Prompt.Length, _height);
        }

        public void Start()
        {
            SetPrompt(".");
        }

        public string Prompt { get; private set; }
        
        private void DisplayPrompt()
        {
            _consoleAbstraction.WriteOnPosition(Prompt, 0, _height, ConsoleColor.Green);
        }
    }
}