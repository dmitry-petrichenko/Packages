using System;
using System.Threading.Tasks;
using RemoteApi;

namespace Client2
{
    class Program
    {
        private static IConsolePromptChat _consolePromptChat;
        private static IRemoteApiOperator _remoteOperator;
        
        static async Task Main(string[] args)
        {
            var factory = new RemoteApiOperatorFactory();
            _remoteOperator = factory.Create();
            _remoteOperator.Error += e => Console.WriteLine(e);
            _remoteOperator.ConnectedSuccess += OnConnectedSuccess;
            
            _consolePromptChat = new ConsolePromptChat();
            _consolePromptChat.CommandReceived += OnCommandReceived;
            _consolePromptChat.ReadFromInput();

            await new TaskCompletionSource<bool>().Task;
        }

        private static void OnConnectedSuccess(string connected)
        {
            _consolePromptChat.Prompt = connected;
        }

        private static async Task OnCommandReceived(string command)
        {
            await _remoteOperator.ExecuteCommand(command);
        }
    }

    public class RemoteApiOperatorFactory
    {
        public IRemoteApiOperator Create()
        {
            var instructionsSenderFactory = new InstructionsSenderFactory();
            var remoteApiOperator = new RemoteApiOperator(instructionsSenderFactory);

            return remoteApiOperator;
        }
    }
}