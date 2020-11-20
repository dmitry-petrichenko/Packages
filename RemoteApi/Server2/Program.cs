using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionProtocol;
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
            var recorder = new DefaultRecorder { ShowErrors = true, ShowInfo = true };
            var instructionReceiver = new InstructionsReceiver(new NodeGatewayFactory(recorder), new NetworkAddress(address), recorder);
            var remoteMapApi = new RemoteApiMap(instructionReceiver);
            
            return remoteMapApi;
        }
    }
}