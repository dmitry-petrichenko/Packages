using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;

namespace Experiments
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var resutl = SendInstruction(Enumerable.Empty<byte>());

            Console.ReadLine();
        }

        public static async Task SendImitation(string element)
        {
            Console.WriteLine($"send {element}");
            await Task.Delay(1000);
        }
        
        public static Task<(bool, IEnumerable<byte>)> SendInstruction(IEnumerable<byte> instruction)
        {
            return SafeExecution.TryCatchWithResultAsync(() => SendInstructionInternal(instruction),
                exception => Console.WriteLine("ex"));
        }
        
        public static Task<(bool, IEnumerable<byte>)> SendInstructionInternal(IEnumerable<byte> instruction)
        {
            throw new Exception();
        }
    }
}