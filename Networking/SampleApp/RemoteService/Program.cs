using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Networking.RemoteApiPlugin;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.Storage.QueuesStorage;

namespace SampleService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateApplicationBuilder().Build(SetupCoreHandler, "appsettings.json").Run();
            
            Console.ReadLine();
        }

        private static IRunnable SetupCoreHandler(ITraceableRemoteApiMap map, IApplicationRecorder recorder, IStorage storage)
        {
            foreach (var message in recorder.GetCache())
            {
                Console.WriteLine(message);
            }

            recorder.RecordReceived += s => Console.WriteLine(s);

            // Create core logic here
            var core = new UsefulLogic(recorder);

            var storage1 = new StorageLogic(storage);
            
            // Register commands here
            map.RegisterCommandWithParameters("set", parameter =>
            {
                core.SetValue(Int32.Parse(parameter.FirstOrDefault()));
            });
            
            map.RegisterCommandWithParameters("enq", parameter =>
            {
                storage1.AddValue(parameter.FirstOrDefault());
                recorder.RecordInfo("app", $"enqueued value: {parameter.FirstOrDefault()}");
            });
            
            map.RegisterCommandWithParameters("cur", parameter =>
            {
                var cur = storage1.GetCurrent();
                recorder.RecordInfo("app", $"current value: {cur}");
            });
            
            map.RegisterCommandWithParameters("deq", parameter =>
            {
                var cur = storage1.PopValue();
                recorder.RecordInfo("app", $"dequeued value: {cur}");
            });
            
            return core;
        }

        public static IServiceBuilder CreateApplicationBuilder()
        {
            return new ServiceBuilder();
        }
    }
}