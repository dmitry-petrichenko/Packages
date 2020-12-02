using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Networking.NetworkExtensions;
using RemoteApi;

namespace C8F2740A.NetworkNode.RemoteTerminalTracer
{
    public class RemoteTerminalTracerServer
    {
        private readonly IRemoteApiMap _remoteApiMap;
        private readonly IRemoteApiOperator _remoteApiOperator;
        
        public RemoteTerminalTracerServer(
            IRemoteApiMap remoteApiMap,
            IRemoteApiOperator remoteApiOperator)
        {
            _remoteApiMap = remoteApiMap;
            _remoteApiOperator = remoteApiOperator;
            
            _remoteApiMap.RegisterCommandWithParameters("traceto",
                TraceToHandler, "<ip:port>");
        }

        private IEnumerable<byte> TraceToHandler(IEnumerable<string> arguments)
        {
            if (arguments.Count() != 1)
            {
                return Encoding.ASCII.GetBytes("wrong parameters");
            }
            
            if (!arguments.First().IsCorrectIPv4Address())
            {
                return Encoding.ASCII.GetBytes("wrong parameters");
            }

            ConnectAndSend(arguments.First());

            return Enumerable.Empty<byte>();
        }

        private async Task ConnectAndSend(string address)
        {
            await _remoteApiOperator.ExecuteCommand($"connect {address}");
            _remoteApiOperator.ExecuteCommand("hello");
        }
    }
}