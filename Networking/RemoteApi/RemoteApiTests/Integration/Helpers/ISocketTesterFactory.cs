using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace RemoteApi.Integration.Helpers
{
    public interface ISocketTesterFactory
    {
        ISocket Create();
    }
}