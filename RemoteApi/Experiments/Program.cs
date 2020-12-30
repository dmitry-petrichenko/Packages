using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Experiments
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var s = new SemaphoreSlim(1);
            async void Print()
            {
                await s.WaitAsync();
                Console.WriteLine($"1 {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"2 {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"3 {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"4 {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"5 {Thread.CurrentThread.ManagedThreadId}");
                s.Release();
            }

            Task.Run(async () =>
            {
                Print();
                await Task.Delay(800);
            });
            
            Task.Run(async () =>
            {
                Print();
                await Task.Delay(700);
            });
            
            Task.Run(async () =>
            {
                Print();
                await Task.Delay(600);
            });

            Console.ReadLine();
        }
    }
}