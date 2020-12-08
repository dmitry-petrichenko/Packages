using System;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface INodeGateway
    {
        event Action<ISession> ConnectionReceived;
    }
}