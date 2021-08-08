using System;
using System.Collections.Generic;
using System.Linq;
using RemoteApi;

namespace C8F2740A.NetworkNode.RemoteTerminalTracer
{
    public class RemoteTerminalClient
    {
        private readonly IRemoteApiMap _remoteApiMap;
        private readonly IRemoteApiOperator _remoteApiOperator;
        
        public RemoteTerminalClient(
            IRemoteApiMap remoteApiMap,
            IRemoteApiOperator remoteApiOperator)
        {
            _remoteApiMap = remoteApiMap;
            _remoteApiOperator = remoteApiOperator;
            
            _remoteApiMap.RegisterCommand("hello", HelloHandler);
            
        }

        private IEnumerable<byte> HelloHandler()
        {
            Console.WriteLine("say hello");
            
            return Enumerable.Empty<byte>();
        }
    }
}