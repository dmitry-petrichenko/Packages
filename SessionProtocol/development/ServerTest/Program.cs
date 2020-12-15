﻿using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new DefaultInstructionReceiverFactory();
            var instructionReceiver = factory.Create("127.0.0.1:12091");

            instructionReceiver.InstructionReceived += InstructionReceivedHandler;

            Console.ReadLine();
        }

        private static IEnumerable<byte> InstructionReceivedHandler(IEnumerable<byte> instruction)
        {
            var str = System.Text.Encoding.Default.GetString(instruction.ToArray());
            Console.WriteLine(str);
            
            return instruction;
        }
    }
}