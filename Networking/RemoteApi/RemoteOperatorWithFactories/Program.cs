using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi;

namespace RemoteOperatorWithFactories
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateApplicationBuilder().Build().Run();
        }

        public static IApplicationBuildable CreateApplicationBuilder()
        {
            return new ApplicationBuilder();
        }
    }
}