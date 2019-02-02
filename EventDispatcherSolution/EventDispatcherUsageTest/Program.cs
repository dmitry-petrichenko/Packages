using System;
using ID5D6AAC.Common.EventDispatcher;

namespace ConsoleApp1
{
    class Program
    {
        public const string EVENT_FIRST = "Event1";
        public const string EVENT_SECOND = "Event2";
        
        static void Main(string[] args)
        {
            IEventDispatcher eventDispatcher = new EventDispatcher();
            eventDispatcher.AddEventListener<string>(EVENT_FIRST, WrightEventFirstToConsoleString);
            eventDispatcher.AddEventListener<string>(EVENT_FIRST, WrightEventFirstToConsoleString);
            eventDispatcher.AddEventListener<string>(EVENT_FIRST, WrightEventFirstToConsoleString);
            
            //eventDispatcher.AddEventListener(EVENT_SECOND, WrightEventFirstToConsole);
            eventDispatcher.RemoveEventListener(EVENT_FIRST, new Action<string>(WrightEventFirstToConsoleString));
            //eventDispatcher.RemoveEventListener(EVENT_FIRST, new Action(WrightEventFirstToConsole));
            
            eventDispatcher.DispatchEvent(EVENT_FIRST, "hello world");
            //eventDispatcher.DispatchEvent(EVENT_FIRST);
            
            //eventDispatcher.RemoveEventListener(EVENT_FIRST, new Action(Wright2));s
            //eventDispatcher.DispatchEvent(EVENT_FIRST);
        }

        private static void WrightEventFirstToConsoleString(string value)
        {
            Console.WriteLine(value);
        }

        static void WrightEventFirstToConsole()
        {
            Console.WriteLine("Event1");
        }
    }
}