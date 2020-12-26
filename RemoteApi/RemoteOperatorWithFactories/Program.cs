using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace RemoteOperatorWithFactories
{
    class Program
    {
        private static IApplicationRecorder _recorder;
        private static IRemoteTraceMonitor _remoteTraceMonitor;
        private static ITextToRemoteSender _textToRemoteSender; 
        
        static async Task Main(string[] args)
        {
            var consoleAbstraction = new ConsoleAbstraction();
            _remoteTraceMonitor = new RemoteTraceMonitorMock();//new RemoteTraceMonitor(consoleAbstraction, 4);
            _remoteTraceMonitor.Start();
            
            var recorder = new ApplicationRecorder(new SystemRecorder(), new MessagesCache(10));
            _recorder = recorder;
            var instructionReceiverFactory = new DefaultInstructionReceiverFactory(recorder);
            var instructionReceiver = instructionReceiverFactory.Create("127.0.0.1:10000");
            var remoteApiMap = new RemoteApiMap(instructionReceiver, recorder);
            _textToRemoteSender = remoteApiMap;
            var consistentMessageSender = new СonsistentMessageSender(remoteApiMap, recorder);

            var remoteRecorderSender = new RemoteRecordsSender(consistentMessageSender, recorder, recorder);

            var instructionSenderHolder = new InstructionSenderHolder(recorder);
            var remoteApiOperator = new RemoteApiOperator(
                instructionSenderHolder, 
                new DefaultInstructionSenderFactory(recorder),
                recorder);
            
            var connectParser = new ConnectParser(remoteApiOperator, recorder);
            var autoLocalConnector = new AutoLocalConnector(connectParser, recorder);
            
            var application = new Application(autoLocalConnector, _remoteTraceMonitor);
            
            var traceableRemoteApiMap = new TraceableRemoteApiMap(remoteApiMap, remoteRecorderSender);
            traceableRemoteApiMap.RegisterCommand("sayhello", SayHelloHandler);
            traceableRemoteApiMap.RegisterWrongCommandHandler(WrongCommandHandler);
            
            await application.Start();
            //-----------------------------------------------------------------------
            
            await new TaskCompletionSource<bool>().Task;
        }

        private static void WrongCommandHandler()
        {
            //var r = await _textToRemoteSender.TrySendText("Wrong");
            _recorder.RecordInfo("", "Wrong");
        }

        private static void SayHelloHandler()
        {
            _recorder.RecordInfo("", "Hello");
        }

        private class SystemRecorder : ISystemRecorder
        {
            public void Record(string message)
            {
                _remoteTraceMonitor.DisplayDebugMessage(message);
            }
        }
        
        private class RemoteTraceMonitorMock : IRemoteTraceMonitor
        {
            public void Start()
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var line = Console.ReadLine();
                        TextEntered?.Invoke(line);
                    }
                });
            }

            public void Stop()
            {
                
            }

            public void DisplayNextMessage(string message)
            {
                Console.WriteLine($"(rmt):{message}");
            }

            public void DisplayDebugMessage(string message)
            {
                Console.WriteLine($"(dbg):{message}");
            }

            public void ClearTextBox()
            {

            }

            public void SetPrompt(string value)
            {
                
            }

            public event Action<string> TextEntered;
        }
    }
}