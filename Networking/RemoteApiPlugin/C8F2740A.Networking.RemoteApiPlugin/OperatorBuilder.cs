using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Factories;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public interface IOperatorBuildable
    {
        IOperatorRunnable Build(string settingsPath);
    }
    
    public interface IOperatorRunnable
    {
        Task Run();
    }
    
    public class OperatorBuilder : IOperatorBuildable, IOperatorRunnable
    {
        private TaskCompletionSource<OperatorExecutionResult> _mainApplicationTask;
        private IConfigurationRoot _configuration;

        public IOperatorRunnable Build(string settingsPath)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            return this;
        }

        public Task Run()
        {
            _mainApplicationTask = new TaskCompletionSource<OperatorExecutionResult>();

            // Recorder factory
            var recorderFactory = new RecorderFactory(_configuration, SystemInterruptedHandler);
            
            // System recorder
            var systemRecorder = recorderFactory.CreateSystemRecorder();

            // Application recorder
            var applicationRecorder = recorderFactory.CreateApplicationRecorder();
            
            // Remote trace monitor
            var remoteTraceMonitor = new RemoteTraceMonitor(new ConsoleAbstraction()
                ,Int32.Parse(_configuration["MONITOR_LINES"]),
                bool.Parse(_configuration["SHOW_DEBUG_MESSAGES"]),
                systemRecorder, 
                systemRecorder);
            remoteTraceMonitor.Start();
            var remoteTraceMonitorСonsistent = new RemoteTraceMonitorСonsistent(remoteTraceMonitor);
            
            var remoteOperatorFactory = new BaseMonitoredRemoteOperatorFactory(new BaseInstructionSenderFactory(applicationRecorder), remoteTraceMonitorСonsistent, applicationRecorder, applicationRecorder);
            var traceableRemoteApiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(applicationRecorder), applicationRecorder);

            var apiOperatorFactory = new ApiOperatorFactory(remoteOperatorFactory, traceableRemoteApiMapFactory, applicationRecorder);
            var apiOperator = apiOperatorFactory.Create(_configuration["IP_ADDRESS"]);
            apiOperator.Finished += FinishedHandler;
            
            return _mainApplicationTask.Task;
        }
        
        private void FinishedHandler()
        {
            _mainApplicationTask.SetResult(OperatorExecutionResult.Exit);
        }

        private void SystemInterruptedHandler(string message)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

            Console.ReadKey();
            _mainApplicationTask.SetResult(OperatorExecutionResult.Error);
        }
    }
}