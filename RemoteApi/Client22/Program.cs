using System;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace Client22
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new DefaultRecorder(new DefaultRecorderSettings());
            var f = new BaseInstructionReceiverFactory(r);
            var res = f.Create("127.0.0.1:11000");

            Console.ReadLine();
        }
    }
}