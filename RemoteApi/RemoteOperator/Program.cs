using System;
using C8F2740A.Common.Records;
using RemoteApi.Trace;

namespace RemoteOperator
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateApplicationBuilder().Build().Run();
            Console.ReadLine();
        }
        
        private static ApplicationBuilder CreateApplicationBuilder()
        {
            return new ApplicationBuilder();
        }
    }
}