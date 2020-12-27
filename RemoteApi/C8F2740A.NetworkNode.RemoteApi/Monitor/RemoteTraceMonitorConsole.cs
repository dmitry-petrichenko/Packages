using System;
using System.Threading.Tasks;

namespace RemoteApi.Monitor
{
    public class RemoteTraceMonitorToConsole : IRemoteTraceMonitor
    {
        public void Start()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    TextEntered?.Invoke(line);
                }
            });
        }

        public void Stop() { }

        public void DisplayNextMessage(string message)
        {
            Console.WriteLine($"(rmt):{message}");
        }

        public void DisplayDebugMessage(string message)
        {
            Console.WriteLine($"(dbg):{message}");
        }

        public void ClearTextBox()
        {
            Console.Clear();
        }

        public void SetPrompt(string value)
        {
            Console.WriteLine($"SetPrompt:{value}");
        }

        public event Action<string> TextEntered;
    }
}