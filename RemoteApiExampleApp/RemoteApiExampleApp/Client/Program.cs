using System.Threading.Tasks;
using RemoteOperatorWithFactories;

namespace Client
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