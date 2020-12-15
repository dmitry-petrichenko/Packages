using System;
using System.Text;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new DefaultInstructionSenderFactory();
            var instructionSender = factory.Create("127.0.0.1:12091");

            while (true)
            {
                var str = Console.ReadLine();
                instructionSender.TrySendInstruction(Encoding.ASCII.GetBytes(str));
            }
        }
    }
}