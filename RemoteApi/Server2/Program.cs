using System;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace Server2
{
    class Program
    {
        private static int i = 0;
        private static IRemoteTraceMonitor _remoteTraceMonitor;
        
        private class S : IRecorderSettings
        {
            public bool ShowErrors => true;
            public bool ShowInfo => true;
        }
        
        static async Task Main(string[] args)
        {
            var f = new DefaultInstructionSenderFactory(new DefaultRecorder(new S()));
            var i = f.Create("127.0.0.1:10000");
            var result = await i.TrySendInstruction("hello".ToEnumerableByte());
            /*
            var consoleAbstraction = new ConsoleAbstraction();
            _remoteTraceMonitor = new RemoteTraceMonitor(consoleAbstraction, 5);
            _remoteTraceMonitor.Start();
            _remoteTraceMonitor.SetPrompt("127.0.0.1:10000");
            _remoteTraceMonitor.TextEntered += TextEnteredHandler;
            //var textBox = new ConsoleTextBox(consoleAbstraction, 5, 4);
            
            await Task.Delay(1000000);
            */
        }

        private static void TextEnteredHandler(string _)
        {
            i++;
            _remoteTraceMonitor.DisplayNextMessage($"{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}");
            _remoteTraceMonitor.DisplayDebugMessage($"{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}");
            _remoteTraceMonitor.DisplayNextMessage($"{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}{i}");
        }
    }
}