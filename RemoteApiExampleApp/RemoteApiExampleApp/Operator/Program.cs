using System.Threading.Tasks;

namespace Operator
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