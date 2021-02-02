using System;
using System.Threading.Tasks;

namespace C8F2740A.NetworkNode.RemoteApi.Monitor
{
    public interface IConsoleAbstraction
    {
        void SetCursorPosition(int left, int top);
        void ClearLine(int left, int top);
        void DrawSeparatorOnLine(int top, string pattern);
        Task<ConsoleKeyInfo> ReadKeyAsync();
        void WriteOnPosition(string value, int left = 0, int top = 0, ConsoleColor color = ConsoleColor.White);
        void Clear();
        void WriteLine(string value);
    }
    
    public class ConsoleAbstraction : IConsoleAbstraction
    {
        public void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        public void ClearLine(int left, int top)
        {
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', Console.WindowWidth - 1)); 
            Console.SetCursorPosition(left, top);
        }

        public void DrawSeparatorOnLine(int top, string pattern)
        {
            SetCursorPosition(0, top);
            for (int i = 0; i < Console.WindowWidth - 1; i++)
            {
                Console.Write(pattern);
            }
        }

        public Task<ConsoleKeyInfo> ReadKeyAsync()
        {
            return Task.Run(() => Console.ReadKey(true));
        }

        public void WriteOnPosition(string value, int left = 0, int top = 0, ConsoleColor color = ConsoleColor.White)
        {
            SetCursorPosition(left, top);
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }
}