using System;
using System.Linq;
using C8F2740A.NetworkNode.SessionProtocol;

namespace ConsoleApp1
{
    class Program
    {
        private static byte REQUEST = 0b1100_0000;
        private static byte RESPONSE = 0b0011_0000;
        
        static void Main(string[] args)
        {
            var arr = new string[] {"a", "b", "c" };

            var res = arr.Skip(1).ToArray();

            Console.Read();
        }
    }
}