using System;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Storage.DataBase1
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //WriteData();
            //ReadData();
            ReadCurrent();
        }
        
        private static void WriteData()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var factory = new StorageFactory();
            var storage = factory.Create("appsettings.json");
            var q = storage.GetQueue("q");
            q.Enqueue("a1");
            q.Enqueue("a2");
            q.Enqueue("a3");
            storage.Dispose();

            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static void ReadData()
        {
            var factory = new StorageFactory();
            var storage = factory.Create("appsettings.json");
            var q = storage.GetQueue("q");
            Console.WriteLine(q.Dequeue().Item2);
            Console.WriteLine(q.Dequeue().Item2);
            storage.Dispose();
            
            Console.WriteLine("done");
            Console.ReadLine();
        }
        
        private static void ReadCurrent()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var storage = new Storage(configuration);
            var q = storage.GetQueue("q");
            Console.WriteLine(q.GetCurrent().Item2);
            storage.Dispose();

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}