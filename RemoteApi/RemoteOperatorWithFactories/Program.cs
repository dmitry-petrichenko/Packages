using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Nuget.Trace;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace RemoteOperatorWithFactories
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var recorderStream = new RecorderStream();
            var messageStreamer = new MessageStreamer(recorderStream);
            messageStreamer.SetLocalStreaming(false);
            var instructionReceiverFactory = new DefaultInstructionReceiverFactory(recorderStream);
            var instructionReceiver = instructionReceiverFactory.Create("127.0.0.1:55555");
            var remoteApiMap = new RemoteApiMap(instructionReceiver);

            var externalConsolePoint = new ExternalConsolePoint(remoteApiMap, messageStreamer);
            var localConsolePoint = new LocalConsolePoint(externalConsolePoint);
            //-----------------------------------------------------------------------

            var instructionSenderHolder = new InstructionSenderHolder(recorderStream);
            var instructionSenderFactory = new DefaultInstructionSenderFactory(recorderStream);
            var remoteApiOperator = new RemoteApiOperator(instructionSenderHolder, instructionSenderFactory);
            
            //var r = await remoteApiOperator.ExecuteCommand("connect 127.0.0.1:55555");
            var consoleOperatorBootstrapper = new ConsoleOperatorBootstrapper(remoteApiOperator);
            var remoteTraceMonitor = new RemoteTraceMonitor(4);
            //remoteTraceMonitor.Start();
           // remoteTraceMonitor.SetPrompt("text");
           // remoteTraceMonitor.ClearTextBox();

/*test
            var cache = "1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n6\r\n7\r\n";
            var cache2 = "fg\r\nty\r\nfgh\r\nyui\r\njkl\r\njkl\r\nkl;\r\nkl7\r\n";
            remoteTraceMonitor.DisplayNextMessage(cache);
            remoteTraceMonitor.DisplayNextMessage(cache2);
*/
            var traceMonitorFacade = new TraceMonitorFacade(remoteTraceMonitor, consoleOperatorBootstrapper, recorderStream);
            traceMonitorFacade.Start();
            //Console.Write(WriteCache(messageStreamer.GetCache()));
            await Task.Delay(100_000);
        }

        private static void OnConnected(string address, string cache)
        {
            Console.WriteLine("----------------------------");
            Console.WriteLine($"{address}\n{cache}");
        }
        
        public static string WriteCache(IEnumerable<string> values)
        {
            var result = string.Empty;
            
            foreach (var s in values)
            {
                result = s + Environment.NewLine;
            }

            return result;
        }
    }
}