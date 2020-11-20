using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface INetworkAddressFactory
    {
        INetworkAddress Create(string address);
    }
    
    public class NetworkAddressFactory : INetworkAddressFactory
    {
        public INetworkAddress Create(string address)
        {
            return new NetworkAddress(address);
        }
    }
}