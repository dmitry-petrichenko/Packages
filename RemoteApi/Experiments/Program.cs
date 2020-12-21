using System;
using System.Collections.Generic;
using RemoteApi.Trace;

namespace Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new RecorderStream();
            var i = 0;

            while (i < 7)
            {
                r.RecordError("Class", $"Message{i}");
                i++;
            }

            foreach (var s in r.GetCache())
            {
                Console.WriteLine(s);
            }

            Console.ReadKey();
        }
    }

    public static class Extensions
    {
        public static string ToS(this IEnumerable<int> value)
        {
            var result = string.Empty;
            
            foreach (var i in value)
            {
                result += $"{i} ";
            }

            return result;
        }
    }
}