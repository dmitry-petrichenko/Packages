using System;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface INodeGateway
    {
        event Action<ISession> ConnectionReceived;
    }
}