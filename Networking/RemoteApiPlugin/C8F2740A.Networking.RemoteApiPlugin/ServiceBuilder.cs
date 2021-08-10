using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public class ServiceBuilder : IServiceBuilder, IServiceRunner
    {
        private TaskCompletionSource<bool> _mainApplicationTask;
        private IRunnable _core;
        
        public ServiceBuilder()
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
        }

        public IServiceRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IRunnable> setupCore)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            // Recorder factory
            var recorderFactory = new RecorderFactory(configuration, SystemInterruptedHandler);
            
            // Application recorder
            var applicationRecorder = recorderFactory.CreateApplicationRecorder();
            
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);
            var map = traceableRemoteApiMapFactory.Create(configuration["IP_ADDRESS"]);

            _core = setupCore?.Invoke(map, applicationRecorder);
            
            return this;
        }

        public Task Run()
        {
            _core.Run();
            
            return _mainApplicationTask.Task;
        }
        
        private void SystemInterruptedHandler(string message)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

            Console.ReadKey();
            _mainApplicationTask.SetResult(false);
        }
    }
}