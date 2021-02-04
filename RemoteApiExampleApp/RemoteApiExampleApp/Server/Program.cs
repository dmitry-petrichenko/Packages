using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteOperatorWithFactories;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = Console.ReadLine();
            
            var systemRecorder = new SystemRecorder();
            var systemMessageDispatcher = systemRecorder;
            systemMessageDispatcher.InterruptedWithMessage += SystemInterruptedHandler;
            
            // Application recorder
            var applicationRecorder = new ApplicationRecorder(systemRecorder, new MessagesCache(10));
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);
            var map = traceableRemoteApiMapFactory.Create($"127.0.0.1:{port}");

            applicationRecorder.RecordReceived += s => Console.WriteLine(s);
            var usefulLogic = new UsefulLogic(applicationRecorder);
            
            map.RegisterCommandWithParameters("set", param => usefulLogic.SetValue(Int32.Parse(param.FirstOrDefault())));

            Console.ReadLine();
        }

        private static void SystemInterruptedHandler(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
        }

        private class UsefulLogic
        {
            private readonly IApplicationRecorder _recorder;
            private int _currentValue;
            
            public UsefulLogic(IApplicationRecorder recorder)
            {
                _recorder = recorder;
                _currentValue = 0;
                StartProcess();
            }
            
            public void SetValue(int value)
            {
                _currentValue = value;
            }

            private async Task StartProcess()
            {
                while (_currentValue < Int32.MaxValue)
                {
                    await Task.Delay(800);
                    _currentValue++;
                    _recorder.RecordInfo("App", $"value: {_currentValue}");
                }
            }
        }
    }
}