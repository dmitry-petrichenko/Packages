using System;
using System.Threading.Tasks;

namespace RemoteApi
{
    public interface IConsolePromptChat
    {
        string Prompt { set; }

        void ReadFromInput();

        event Func<string, Task> CommandReceived;
    }
    
    public class ConsolePromptChat : IConsolePromptChat
    {
        private string _prompt;
        
        public ConsolePromptChat()
        {
            Prompt = ">>>";
        }

        public void ReadFromInput()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Console.Write(Prompt);
                    var line = Console.ReadLine();
                    await CommandReceived?.Invoke(line);
                }
            });
        }

        public string Prompt
        {
            get => _prompt;
            set
            {
                _prompt = value;
                _prompt += " ";
            }
        }
        
        public event Func<string, Task> CommandReceived;
    }
}