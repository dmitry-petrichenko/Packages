using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using C8F2740A.Storages.QueuesStorage;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public class ServiceBuilder : IServiceBuilder, IServiceRunner
    {
        private TaskCompletionSource<bool> _mainApplicationTask;
        private IUpable _core;
        
        public ServiceBuilder()
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
        }

        public IServiceRunner Build(
            Func<ITraceableRemoteApiMap, IApplicationRecorder, IStorage,
            IUpable> setupCore,
            string settingsPath)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsPath)
                .Build();
            
            // Recorder factory
            var recorderFactory = new RecorderFactory(configuration, SystemInterruptedHandler);
            
            // Application recorder
            var applicationRecorder = recorderFactory.CreateApplicationRecorder();
            
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);
            var map = traceableRemoteApiMapFactory.Create(configuration["IP_ADDRESS"]);
            var storage = new StorageFactory().Create("appsettings.json");
            
            RegisterBaseCommands(map);
            
            _core = setupCore?.Invoke(map, applicationRecorder, storage);
            
            return this;
        }

        public Task Run()
        {
            _core.Up();
            
            return _mainApplicationTask.Task;
        }

        private void RegisterBaseCommands(ITraceableRemoteApiMap traceableRemoteApiMap)
        {
            traceableRemoteApiMap.RegisterCommand("complete", () =>
            {
                _mainApplicationTask.TrySetResult(true);
            });
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