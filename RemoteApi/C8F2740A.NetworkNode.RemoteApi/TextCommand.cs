using System.Collections.Generic;
using System.Linq;

namespace RemoteApi
{
    public interface ITextCommand
    {
        string Command { get; }
        IEnumerable<string> Parameters { get; }
    }
    
    public class TextCommand : ITextCommand
    {
        public TextCommand(string command)
        {
            var array = command.Split(" ");
            Command = array.First();
            Parameters = array.Skip(1).ToArray();
        }

        public string Command { get; }
        public IEnumerable<string> Parameters { get; }
    }
}