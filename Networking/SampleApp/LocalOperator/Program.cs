using System.Threading.Tasks;
using C8F2740A.Networking.RemoteApiPlugin;

namespace Operator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateOperatorBuilder().Build("appsettings.json").Run();
        }

        public static IOperatorBuildable CreateOperatorBuilder()
        {
            return new OperatorBuilder();
        }
    }
}