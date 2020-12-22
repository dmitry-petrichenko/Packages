using System;
using System.Threading.Tasks;

namespace RemoteApi.Trace
{
    public interface IAsyncLineReader
    {
        Task<string> ReadLineOnPositionAsync(Func<int> leftIndentResolver, int top);
    }
    
    public class AsyncLineReader : IAsyncLineReader
    {
        private readonly IConsoleAbstraction _consoleAbstraction;
        
        public AsyncLineReader(IConsoleAbstraction consoleAbstraction)
        {
            _consoleAbstraction = consoleAbstraction;
        }

        public async Task<string> ReadLineOnPositionAsync(Func<int> leftIndentResolver, int top)
        {
            var input = string.Empty;
            
            while (true)
            {
                _consoleAbstraction.SetCursorPosition( leftIndentResolver.Invoke() + input.Length, top);

                var key = await _consoleAbstraction.ReadKeyAsync();
                _consoleAbstraction.ClearLine(leftIndentResolver.Invoke(), top);

                if (key.Key == ConsoleKey.Enter)
                {
                    return input;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Remove(input.Length -1);
                    }
                }
                else
                {
                    input += key.KeyChar;
                }
                
                _consoleAbstraction.WriteOnPosition(input, leftIndentResolver.Invoke(), top);
            }
        }
    }
}