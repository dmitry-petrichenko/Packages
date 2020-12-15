using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace ClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new DefaultInstructionSenderFactory();
            var instructionSender = factory.Create("127.0.0.1:12091");
            instructionSender.InstructionReceived += InstructionReceivedHandler;

            while (true)
            {
                var str = Console.ReadLine();
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