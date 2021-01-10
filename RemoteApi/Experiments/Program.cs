using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;

namespace Experiments
{
    class Program
    {
        public static BlockingCollection<int> _collection;
        
        static async Task Main(string[] args)
        {
            Console.ReadLine();
        }

        public static void Consume()
        {
            while (!_collection.IsAddingCompleted)
            {
                try
                {
                    Console.Write("");
                    var elements = _collection.Take();
                    Console.WriteLine($"Consumed {elements}");
                    Console.Write("");
                    Console.Write("");
                    Console.Write("");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}