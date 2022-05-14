using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Networking.RemoteApiPlugin;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Microsoft.Extensions.Configuration;

namespace SampleService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var application = CreateApplicationBuilder().Build(SetupCoreHandler, "appsettings.json");
            await application.Run();
            // no readline after this line
        }

        private static IUpable SetupCoreHandler(
            ITraceableRemoteApiMap map, 
            IApplicationRecorder recorder, 
            IConfiguration configuration)
        {
            foreach (var message in recorder.GetCache())
            {
                Console.WriteLine(message);
            }

            recorder.RecordReceived += s => Console.WriteLine(s);

            // Create core logic here
            var core = new UsefulLogic(recorder);
            
            
            // Register commands here
            map.RegisterCommand("trace", () =>
            {
                recorder.RecordInfo("trace", "trace");
            });
            
            return core;
        }

        public static IServiceBuilder CreateApplicationBuilder()
        {
            return new ServiceBuilder();
        }
    }
}