using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace Server3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = new TaskCompletionSource<bool>();

            var cms = new СonsistentMessageSender(new TextToRemoteSender(source), new DefaultRecorder(new DefaultRecorderSettings()));
            Console.WriteLine($"before send {Thread.CurrentThread.ManagedThreadId}");
            cms.SendRemote("text");

            await Task.Delay(4000);
            Console.WriteLine($"before SetResult {Thread.CurrentThread.ManagedThreadId}");
            source.SetResult(true);
            Console.WriteLine("released");
            
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