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
            int[] arr = {1, 2, 3};
            byte[] bytes = { 0b0000_0000 };
            ChangeArray(arr);
            ChangeArray2(bytes);
            Console.WriteLine(DateTime.Now.ToShortTimeFormat());
            
            Console.ReadLine();
        }

        public static void ChangeArray(int[] arr)
        {
            int[] b = new int[1];
            b[0] = 7;
            Buffer.BlockCopy(b, 0, arr, 0, 1);
        }
        
        public static void ChangeArray2(byte[] bytes)
        {
            byte[] b = new byte[1];
            b[0] = 7;
            bytes = b;
        }
    }

    public static class E
    {
        public static string ToShortTimeFormat(this DateTime dateTime)
        {
            var seconds = dateTime.Second >= 10 ? dateTime.Second.ToString() : $"0{dateTime.Second}";
            var minutes = dateTime.Minute >= 10 ? dateTime.Minute.ToString() : $"0{dateTime.Minute}";
            var hours = dateTime.Hour >= 10 ? dateTime.Hour.ToString() : $"0{dateTime.Hour}";
            return $"{hours}:{minutes}:{seconds}";
        }
    }
}