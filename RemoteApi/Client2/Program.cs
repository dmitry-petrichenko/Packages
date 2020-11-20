using System.Threading.Tasks;
using RemoteApi;

namespace Client2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new RemoteApiOperatorFactory();
            var remoteOperator = factory.Create();
            remoteOperator.Start();
            
            await new TaskCompletionSource<bool>().Task;
        }
    }

    public class RemoteApiOperatorFactory
    {
        public IRemoteApiOperator Create()
        {
            var instructionsSenderFactory = new InstructionsSenderFactory();
            var remoteApiOperator = new RemoteApiOperator(instructionsSenderFactory);

            return remoteApiOperator;
        }
    }
}