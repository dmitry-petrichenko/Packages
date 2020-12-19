using System;
using System.Threading.Tasks;

namespace RemoteApi.Trace
{
    public interface IAsyncLineReader
    {
        Task<string> ReadLineOnPositionAsync(int left, int top);
    }
    
    public class AsyncLineReader : IAsyncLineReader
    {
        private readonly IConsoleAbstraction _consoleAbstraction;
        
        public AsyncLineReader(IConsoleAbstraction consoleAbstraction)
        {
            _consoleAbstraction = consoleAbstraction;
        }

        public async Task<string> ReadLineOnPositionAsync(int left, int top)
        {
            var input = string.Empty;
            
            while (true)
            {
                _consoleAbstraction.SetCursorPosition( left + input.Length, top);

                var key = await _consoleAbstraction.ReadKeyAsync();
                _consoleAbstraction.ClearLine(left, top);

                if (key.Key == ConsoleKey.Enter)
                {
                    return input;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    input = input.Remove(input.Length -1);
                }
                else
                {
                    input += key.KeyChar;
                }
                
                _consoleAbstraction.WriteOnPosition(input, left, top);
            }
        }
    }
}