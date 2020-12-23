using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Experiments
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var collection = new BlockingCollection<string>();
            for (int i = 0; i < 6; i++)
            {
                collection.Add(i.ToString());
            }

            Task.Run(async () =>
            {
                foreach (var element in collection.GetConsumingEnumerable())
                {
                    await SendImitation(element);
                }
            });

            Console.ReadKey();
            
            for (int i = 0; i < 6; i++)
            {
                collection.Add(i.ToString());
            }
            
            Console.ReadKey();
            Console.ReadKey();
        }

        public static async Task SendImitation(string element)
        {
            Console.WriteLine($"send {element}");
            await Task.Delay(1000);
        }
    }
}