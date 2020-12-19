using System;
using System.Linq;
using System.Threading.Tasks;
using RemoteApi;

namespace Server2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var remoteApiMapFactory = new RemoteApiMapFactory();
            var remoteMap = remoteApiMapFactory.Create("127.0.0.1:56789");
            remoteMap.RegisterCommand("add", () =>
            {
                Console.WriteLine("add");
                return Enumerable.Empty<byte>();
            });
            
            remoteMap.RegisterCommandWithParameters("setup", parameters =>
            {
                Console.WriteLine(parameters.First());
                return Enumerable.Empty<byte>();
            }, "p:15");
            
            await new TaskCompletionSource<bool>().Task;
        }
    }

    public class RemoteApiMapFactory
    {
        public IRemoteApiMap Create(string address)
        {
            var remoteMapApi = new RemoteApiMap(instructionReceiver);
            
            return remoteMapApi;
            
        }
    }
}