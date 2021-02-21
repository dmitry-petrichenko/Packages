using System;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface INodeVisitor : IDisposable
    {
        (bool, ISession) TryConnect(INetworkAddress networkAddress);
    }
}