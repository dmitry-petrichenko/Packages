using System.Threading.Tasks;

namespace Operator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateOperatorBuilder().Build().Run();
        }

        public static IOperatorBuildable CreateOperatorBuilder()
        {
            return new OperatorBuilder();
        }
    }
}