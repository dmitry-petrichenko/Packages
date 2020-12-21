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

    public class Test
    {
        public Test(IRecorder r, IRecorderStream s)
        {
            r.RecordError("e", "error1");
            r.RecordError("e", "error2");
            r.RecordInfo("i", "info1");

            var c = s.GetCache();
            
            Console.WriteLine(c);
        }
    }
}