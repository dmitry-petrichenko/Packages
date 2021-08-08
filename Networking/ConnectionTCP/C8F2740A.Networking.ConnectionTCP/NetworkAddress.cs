using System;
using System.Net;
using System.Text.RegularExpressions;

namespace C8F2740A.Networking.ConnectionTCP
{
    public interface INetworkAddress
    {
        IPAddress IP { get; }
        int Port { get; }
    }
    
    public class NetworkAddress : INetworkAddress
    {
        private int _port;
        private IPAddress _iPV4Address;

        public NetworkAddress(string address)
        {
            Initialize(address);
        }

        public IPAddress IP => _iPV4Address;
        public int Port => _port;

        private void Initialize(string address)
        {
            if (!address.IsCorrectIPv4Address())
            {
                throw new Exception("Incorrect IP address");
            }

            var res = address.Split(":");
            _iPV4Address = IPAddress.Parse(res[0]);
            _port = Int32.Parse(res[1]);
        }
    }
}