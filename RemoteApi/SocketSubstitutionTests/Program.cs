using System;
using System.Net;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network;
using RemoteApi.Integration.Helpers;

namespace SocketSubstitutionTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var socket = new SocketSubstitution(new SockectMock(), "o");
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                socket.Dispose();
                await Task.Delay(1000);
                socket.Dispose();
            });

            await socket.Arrange2(socket.DisposeCalledTimes, 2);
            Console.WriteLine("Complete");

            Console.ReadLine();
        }
        
        private static async Task MainAsync()
        {
            var s = new SocketSubstitution(new SockectMock(), "One");

        }

        private static void OnUpdated(SocketSubstitution socketSubstitution)
        {
            Console.WriteLine($"cnt {socketSubstitution.ConnectCalledTimes.Value}");
            Console.WriteLine($"rcv {socketSubstitution.ReceiveCalledTimes.Value}");
        }
    }

    public class SockectMock : ISocket
    {
        public void Dispose()
        {
        }

        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public bool Connected { get; }
        public void Bind(IPAddress ipAddress, int port)
        {
        }

        public void Connect(IPAddress ipAddress, int port)
        {
        }

        public void Listen(int backlog)
        {
        }

        public void Send(byte[] data)
        {
        }

        public int Receive(byte[] bytes)
        {
            return 0;
        }

        public Task<ISocket> AcceptAsync()
        {
            return Task.FromResult(default(ISocket));
        }

        public void Close()
        {
        }
    }
}