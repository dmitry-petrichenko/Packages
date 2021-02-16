using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteOperatorWithFactories;

namespace C8F2740A.NetworkNode.RemoteApiServerPlugin
{
    public interface IApplicationBuilder
    {
        IApplicationRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IRunable> setupCore);
    }
    
    public interface IApplicationRunner
    {
        Task Run();
    }
    
    public interface IRunable
    {
        void Run();
    }
    
    public class ApplicationSkeleton : IApplicationBuilder, IApplicationRunner
    {
        private TaskCompletionSource<bool> _mainApplicationTask;
        private IRunable _core;
        
        public ApplicationSkeleton()
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
        }

        public IApplicationRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IRunable> setupCore)
        {
            // System recorder
            var systemRecorder = new SystemRecorder();
            systemRecorder.InterruptedWithMessage += SystemInterruptedHandler;
            
            // Application recorder
            var applicationRecorder = new ApplicationRecorder(systemRecorder, new MessagesCache(10));
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                , 6, 
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();
            
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);
            var map = traceableRemoteApiMapFactory.Create($"127.0.0.1:8081");

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