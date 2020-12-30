using System;
using System.Threading.Tasks;
using RemoteApi;
using RemoteApi.Monitor;

namespace Server3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var consoleAbstraction = new ConsoleAbstraction();
            var rtm = new RemoteTraceMonitor(consoleAbstraction, 5);
            rtm.Start();

            var first = true;
            rtm.TextEntered += s =>
            {
                if (first)
                {
                    rtm.DisplayNextMessage(s);
                }
                else
                {
                    rtm.SetPrompt(s);
                }

                first = !first;

            };

            /*
            async void DisplayMessages(int i1, int i2)
            {
                for (int i = i1; i < i2; i++)
                {
                    await Task.Delay(1100);
                    rtm.DisplayNextMessage(i.ToString());
                }
            }
            
            async void DisplayDebugMessages(int i1, int i2)
            {
                for (int i = i1; i < i2; i++)
                {
                    await Task.Delay(900);
                    rtm.DisplayDebugMessage(i.ToString());
                }
            }
            
            async void SetPrompts(int i1, int i2)
            {
                for (int i = i1; i < i2; i++)
                {
                    await Task.Delay(800);
                    rtm.SetPrompt(i.ToString());
                }
            }

            /*
            SetPrompts(10000, 10100);
            DisplayDebugMessages(20000, 20100);
            DisplayMessages(33000, 33100);
            */
            /*
            Task.Run(() => SetPrompts(10000, 10100));
            Task.Run(() => DisplayDebugMessages(20000, 20100));
            Task.Run(() => DisplayMessages(33000, 33100)); 
            */
            await new TaskCompletionSource<bool>().Task;
        }

        private class TextToRemoteSender : ITextToRemoteSender
        {
            private readonly TaskCompletionSource<bool> _source;
            
            public TextToRemoteSender(TaskCompletionSource<bool> source)
            {
                _source = source;
            }

            public async Task<bool> TrySendText(string instruction)
            {
                await _source.Task;
                return true;
            }
        }

        public static async Task Method(TaskCompletionSource<bool> source)
        {
            await Task.Run(async () =>
            {
                await source.Task;
            });
        }
    }

    internal class SystemRecorder : ISystemRecorder
    {
        public void Record(string message)
        {
            Console.WriteLine(message);
        }
    }

    /*
            var recorder = new DefaultRecorder(new DefaultRecorderSettings());
            var sf = new DefaultInstructionSenderFactory(recorder);
            var rf = new DefaultInstructionReceiverFactory(recorder);

            var sender = sf.Create("127.0.0.1:10000");
            var receiver = rf.Create("127.0.0.1:10000");
            receiver.InstructionReceived += bytes =>
            {
                receiver.TrySendInstruction("(i):Wrong".ToEnumerableByte());
                return Enumerable.Empty<byte>();
            };

            while (true)
            {
                var line = Console.ReadLine();
                await sender.TrySendInstruction("u".ToEnumerableByte());
                //await receiver.TrySendInstruction("(i):Wrong".ToEnumerableByte());
            }
     */
}