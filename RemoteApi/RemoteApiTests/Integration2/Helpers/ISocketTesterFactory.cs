using C8F2740A.Networking.ConnectionTCP.Network;

namespace RemoteApi.Integration2.Helpers
{
    public interface ISocketTesterFactory
    {
        ISocket Create();
    }
}