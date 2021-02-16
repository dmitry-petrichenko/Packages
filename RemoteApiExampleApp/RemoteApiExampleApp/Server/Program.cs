using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.RemoteApiServerPlugin;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var port = Console.ReadLine();

            await CreateApplicationBuilder().Build(SetupCoreHandler).Run();
            Console.ReadLine();
        }

        private static IRunable SetupCoreHandler(ITraceableRemoteApiMap map, IApplicationRecorder recorder)
        {
            var core = new UsefulLogic(recorder);
            recorder.RecordReceived += s => Console.WriteLine(s);
            map.RegisterCommandWithParameters("set", parameter =>
            {
                core.SetValue(Int32.Parse(parameter.FirstOrDefault()));
            });
            
            return core;
        }

        public static IApplicationBuilder CreateApplicationBuilder()
        {
            return new ApplicationSkeleton();
        }

        private class UsefulLogic : IRunable
        {
            private readonly IApplicationRecorder _recorder;
            private int _currentValue;
            
            public UsefulLogic(IApplicationRecorder recorder)
            {
                _recorder = recorder;
                _currentValue = 0;
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

            public void Run()
            {
                StartProcess();
            }
        }
    }
}