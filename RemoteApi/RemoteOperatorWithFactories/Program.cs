using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Monitor;
using RemoteApi.Trace;

namespace RemoteOperatorWithFactories
{
    class Program
    {
        private static IApplicationRecorder _recorder;
        private static IRemoteTraceMonitor _remoteTraceMonitor;
        private static IMonitoredRemoteOperator _monitoredRemoteOperator;

        private static TaskCompletionSource<bool> _mainApplicationTask;
        
        static async Task Main(string[] args)
        {
            _mainApplicationTask = new TaskCompletionSource<bool>();
            var systemRecorder = new SystemRecorder(_monitoredRemoteOperator, _mainApplicationTask);
            
            var consoleAbstraction = new ConsoleAbstraction();
            _remoteTraceMonitor = new RemoteTraceMonitor(consoleAbstraction, 4, systemRecorder);
            _remoteTraceMonitor.Start();
            
            var recorder = new ApplicationRecorder(systemRecorder, new MessagesCache(10));
            _recorder = recorder;
            var instructionReceiverFactory = new DefaultInstructionReceiverFactory(recorder);
            var instructionReceiver = instructionReceiverFactory.Create("127.0.0.1:10000");
            var remoteApiMap = new RemoteApiMap(instructionReceiver, recorder);
            var consistentMessageSender = new СonsistentMessageSender(remoteApiMap, recorder);

            var remoteRecorderSender = new RemoteRecordsSender(consistentMessageSender, recorder, recorder);

            var instructionSenderHolder = new InstructionSenderHolder(recorder);
            var remoteApiOperator = new RemoteApiOperator(
                instructionSenderHolder,
                new DefaultInstructionSenderFactory(recorder),
                recorder);
            
            var connectParser = new ConnectParser(remoteApiOperator, recorder);
            var autoLocalConnector = new AutoLocalConnector(connectParser, recorder);
            
            var application = new MonitoredRemoteOperator(autoLocalConnector, _remoteTraceMonitor, recorder);
            
            var traceableRemoteApiMap = new TraceableRemoteApiMap(remoteApiMap, remoteRecorderSender, recorder);
            traceableRemoteApiMap.RegisterCommand("sayhello", SayHelloHandler);
            traceableRemoteApiMap.RegisterWrongCommandHandler(WrongCommandHandler);
            
            application.Start();
            //-----------------------------------------------------------------------
            
            await _mainApplicationTask.Task;
        }

        private static void WrongCommandHandler()
        {
            _recorder.RecordInfo("", "Wrong");
        }

        private static void SayHelloHandler()
        {
            _recorder.RecordInfo("", "Hello");
        }

        public class SystemRecorder : ISystemRecorder
        {
            private IMonitoredRemoteOperator _monitoredRemoteOperator;
            private TaskCompletionSource<bool> _mainApplicationTask;
            
            public SystemRecorder(
                IMonitoredRemoteOperator monitoredRemoteOperator,
                TaskCompletionSource<bool> mainApplicationTask)
            {
                _monitoredRemoteOperator = monitoredRemoteOperator;
                _mainApplicationTask = mainApplicationTask;
            }

            public void RecordInfo(string message)
            {
                _remoteTraceMonitor.DisplayDebugMessage(message);
            }

            public void InterruptWithMessage(string message)
            {
                _monitoredRemoteOperator?.Dispose();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();

                Console.ReadKey();
                _mainApplicationTask.SetResult(false);
            }
        }
    }
}