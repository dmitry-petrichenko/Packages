using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace ClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var t = "fgfg";
            Console.WriteLine("fgfg".GetType().FullName);
            /*
            var factory = new DefaultInstructionSenderFactory();
            var instructionSender = factory.Create("127.0.0.1:12091");
            instructionSender.InstructionReceived += InstructionReceivedHandler;

            Loop(instructionSender);

            await Task.Delay(10000000);*/
        }

        private static async Task Loop(IInstructionSender instructionSender)
        {
            while (true)
            {
                Console.WriteLine($"thread id before read {Thread.CurrentThread.ManagedThreadId}");
                var str = await Task.Run(() => Console.ReadLine()); 
                var result = await instructionSender.TrySendInstruction(Encoding.ASCII.GetBytes(str));

                var str2 = System.Text.Encoding.Default.GetString(result.Item2.ToArray());
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(str2);
                Console.ResetColor();
            }
        }
        
        private static IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> instruction)
        {
            var str = System.Text.Encoding.Default.GetString(instruction.ToArray());
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ResetColor();
            
            return instruction;
        }
    }
}