using System;
using System.Text;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;

namespace C8F2740A.Networking.ConnectionTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            var recorder = new DefaultRecorder();
            
            INetworkTunnel CreateTunnel(ISocket socket)
            {
                return new NetworkTunnel(socket, recorder);
            }
            
            var np = new NetworkPoint(new NetworkAddress("192.168.1.200:60000"), CreateTunnel,
                new SocketFactory(), recorder);

            np.Accepted += OnAccepted;

            Console.ReadLine();
        }

        private static void OnAccepted(INetworkTunnel networkTunnel)
        {
            networkTunnel.Received += bytes =>
            {
                var s = Encoding.ASCII.GetString(bytes);
                Console.WriteLine(s);
            };
            networkTunnel.Send(Encoding.ASCII.GetBytes("hi"));
        }
    }
}