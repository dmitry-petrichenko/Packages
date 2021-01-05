using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Factories;
using RemoteApi.Monitor;

namespace RemoteOperatorWithFactories
{
    public interface IApplicationBuildable
    {
        IApplicationRunnable Build();
    }
    
    public interface IApplicationRunnable
    {
        Task Run();
    }
    
    public class ApplicationBuilder : IApplicationBuildable, IApplicationRunnable
    {
        private TaskCompletionSource<bool> _mainApplicationTask;
        private ISystemMessageDispatcher _systemMessageDispatcher;
        private IApplicationRecorder _applicationRecorder;
        
        public IApplicationRunnable Build()
        {
            return this;
        }

        public Task Run()
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
            
            // System recorder
            var systemRecorder = new SystemRecorder();
            _systemMessageDispatcher = systemRecorder;
            _systemMessageDispatcher.InterruptedWithMessage += SystemInterruptedHandler;
            
            // Application recorder
            _applicationRecorder = new ApplicationRecorder(systemRecorder, new MessagesCache(10));
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                , 4, 
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();
            
            var remoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(new BaseInstructionSenderFactory(_applicationRecorder), remoteTraceMonitor, _applicationRecorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(_applicationRecorder), _applicationRecorder);

            var apiOperatorFactory = new ApiOperatorFactory(systemRecorder, remoteOperatorFactory, traceableRemoteApiMapFactory, _applicationRecorder);
            apiOperatorFactory.Create("127.0.0.1:8081");
            
            return _mainApplicationTask.Task;
        }

        private void SystemInterruptedHandler(string message)
        {
            //.Dispose();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

            Console.ReadKey();
            _mainApplicationTask.SetResult(false);
        }
    }
}