using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Factories;

namespace RemoteApiMapPure
{
    class Program
    {
        private static IApplicationRecorder _applicationRecorder;
        
        static async Task Main(string[] args)
        {
            var port = Console.ReadLine();
            _applicationRecorder = new ApplicationRecorder(new SystemRecorder(), new MessagesCache(10));

            // Remote api
            var apiMapFactory = new BaseTraceableRemoteApiMapFactory(new BaseInstructionReceiverFactory(_applicationRecorder), _applicationRecorder);
            var remoteApiMap2 = apiMapFactory.Create($"127.0.0.1:{port}");
            remoteApiMap2.RegisterWrongCommandHandler(WrongCommandHandler);
            remoteApiMap2.RegisterCommand("command", CommandHandler);
            
            _applicationRecorder.RecordInfo("", "Created");
            
            await new TaskCompletionSource<bool>().Task;
        }

        private static void CommandHandler()
        {
            _applicationRecorder.RecordInfo("", "Command");
        }

        private static void WrongCommandHandler()
        {
            _applicationRecorder.RecordInfo("", "Wrong command");
        }

        private class SystemRecorder : ISystemRecorder
        {
            public void RecordInfo(string message)
            {
                Console.WriteLine(message);
            }

            public void InterruptWithMessage(string message)
            {
                Console.WriteLine(message);
            }

            public event Action<string> InfoMessageReceived;
            public event Action<string> InterruptedWithMessage;
        }
    }
}