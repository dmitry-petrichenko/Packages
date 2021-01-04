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
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                , 4, 
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();

            // Application recorder
            _applicationRecorder = new ApplicationRecorder(systemRecorder, new MessagesCache(10));
            
            // Remote operator 
            var remoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(new BaseInstructionSenderFactory(_applicationRecorder), remoteTraceMonitor, _applicationRecorder);
            var remoteOperator = remoteOperatorFactory.Create("127.0.0.1:10000");
            
            // Remote api
            var apiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(_applicationRecorder), _applicationRecorder);
            var remoteApiMap = apiMapFactory.Create("127.0.0.1:10000");
            remoteApiMap.RegisterWrongCommandHandler(WrongCommandHandler);
            
            remoteOperator.Start();
                
            return _mainApplicationTask.Task;
        }

        private void WrongCommandHandler()
        {
            _applicationRecorder.RecordInfo("App", "Wrong command");
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