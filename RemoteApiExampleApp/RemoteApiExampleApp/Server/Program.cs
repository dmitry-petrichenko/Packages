using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.RemoteApiServerPlugin;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteOperatorWithFactories;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = Console.ReadLine();

            await CreateApplicationBuilder().Build().Run();
            Console.ReadLine();
        }
        
        public static IApplicationBuilder CreateApplicationBuilder()
        {
            return new ApplicationSkeleton();
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