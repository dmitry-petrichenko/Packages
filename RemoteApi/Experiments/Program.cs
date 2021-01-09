using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Experiments
{
    class Program
    {
        public static BlockingCollection<int> _collection;
        
        static async Task Main(string[] args)
        {
            _collection = new BlockingCollection<int>();

            Task.Run(() => Consume());

            await Task.Delay(2000);

            for (int i = 0; i < 10; i++)
            {
                _collection.Add(i);
                await Task.Delay(5000);
            }

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