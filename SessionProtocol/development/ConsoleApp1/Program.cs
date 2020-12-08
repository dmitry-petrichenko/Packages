using System;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        private static byte REQUEST = 0b1100_0000;
        private static byte RESPONSE = 0b0011_0000;
        
        static void Main(string[] args)
        {
            var arr = (false, false);
            ValueTuple v = new ValueTuple();
            Console.WriteLine(arr.GetType().Name);

            //var res = arr.Skip(1).ToArray();

            //Console.Read();
        }
    }
}