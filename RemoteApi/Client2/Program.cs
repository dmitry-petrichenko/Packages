using System;
using System.Threading.Tasks;
using RemoteApi;
using RemoteApi.Trace;

namespace Client2
{
    class Program
    {
        //private static IConsolePromptChat _consolePromptChat;
        private static IRemoteApiOperator _remoteOperator;

        private static async Task Method()
        {
            IConsoleAbstraction console = new ConsoleAbstraction();
            var key = await console.ReadKeyAsync();
            Console.WriteLine($"key entered: {key}");
        }

        static async Task Main(string[] args)
        {
            var rtm = new RemoteTraceMonitor(4);
            rtm.TextEntered += s =>
            {
                if (s[0] == "0"[0])
                {
                    rtm.SetPrompt(s);
                }
                else
                {
                    rtm.DisplayNextMessage(s);
                }
            };
            rtm.Start();
            
            await Task.Delay(100_000);
        }

        private static void OnConnectedSuccess(string connected)
        {
            //_consolePromptChat.Prompt = connected;
        }

        private static async Task OnCommandReceived(string command)
        {
            //_consolePromptChat.Prompt = command;
        }
    }
}