using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RAServicePlugin;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using Microsoft.Extensions.Configuration;

namespace Operator
{
    public interface IOperatorBuildable
    {
        IOperatorRunnable Build();
    }
    
    public interface IOperatorRunnable
    {
        Task Run();
    }
    
    public class OperatorBuilder : IOperatorBuildable, IOperatorRunnable
    {
        private TaskCompletionSource<bool> _mainApplicationTask;

        public IOperatorRunnable Build()
        {
            return this;
        }

        public Task Run()
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            // Recorder factory
            var recorderFactory = new RecorderFactory(configuration, SystemInterruptedHandler);
            
            // System recorder
            var systemRecorder = recorderFactory.CreateSystemRecorder();

            // Application recorder
            var applicationRecorder = recorderFactory.CreateApplicationRecorder();
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                ,Int32.Parse(configuration["MONITOR_LINES"]),
                bool.Parse(configuration["SHOW_DEBUG_MESSAGES"]),
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistent(remoteTraceMonitor);
            
            var remoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(new BaseInstructionSenderFactory(applicationRecorder), remoteTraceMonitorСonsistent, applicationRecorder, applicationRecorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);

            var apiOperatorFactory = new ApiOperatorFactory(remoteOperatorFactory, traceableRemoteApiMapFactory, applicationRecorder);
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