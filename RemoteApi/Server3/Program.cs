using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;

namespace Server3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var source = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"set result thread {Thread.CurrentThread.ManagedThreadId}");
                
                source.SetResult(false);
            });
            
            Console.WriteLine($"before method {Thread.CurrentThread.ManagedThreadId}");
            await Method(source);

            Console.WriteLine($"after method {Thread.CurrentThread.ManagedThreadId}");
            
            await new TaskCompletionSource<bool>().Task;
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