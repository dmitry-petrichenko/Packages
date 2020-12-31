using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace RemoteApiMapPure
{
    class Program
    {
        private static IApplicationRecorder _applicationRecorder;
        
        static async Task Main(string[] args)
        {
            var recorder = new ApplicationRecorder(new SystemRecorder(), new MessagesCache(10));
            _applicationRecorder = recorder;
            var instructionReceiverFactory = new DefaultInstructionReceiverFactory(recorder);
            var instructionReceiver = instructionReceiverFactory.Create("127.0.0.1:11111");
            var remoteApiMap = new RemoteApiMap(instructionReceiver, recorder);
            var consistentMessageSender = new СonsistentMessageSender(remoteApiMap, recorder);
            var remoteRecorderSender = new RemoteRecordsSender(consistentMessageSender, recorder, recorder);
            var traceableRemoteApiMap = new TraceableRemoteApiMap(remoteApiMap, remoteRecorderSender);
            traceableRemoteApiMap.RegisterWrongCommandHandler(WrongCommandHandler);
            traceableRemoteApiMap.RegisterCommand("command", CommandHandler);
            
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
        }
    }
}