using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using Microsoft.Extensions.Configuration;

namespace Operator
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
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            // System recorder
            var systemRecorder = new SystemRecorder();
            _systemMessageDispatcher = systemRecorder;
            _systemMessageDispatcher.InterruptedWithMessage += SystemInterruptedHandler;
            
            // Application recorder
            _applicationRecorder = new ApplicationRecorder(systemRecorder, 
                new MessagesCache(Int32.Parse(configuration["MESSAGE_CACHE"])));
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                ,Int32.Parse(configuration["MONITOR_LINES"]),
                bool.Parse(configuration["SHOW_DEBUG_MESSAGES"]),
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistent(remoteTraceMonitor);
            
            var remoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(new BaseInstructionSenderFactory(_applicationRecorder), remoteTraceMonitorСonsistent, _applicationRecorder, _applicationRecorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(_applicationRecorder), _applicationRecorder);

            var apiOperatorFactory = new ApiOperatorFactory(remoteOperatorFactory, traceableRemoteApiMapFactory, _applicationRecorder);
            apiOperatorFactory.Create(configuration["IP_ADDRESS"]);
            
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