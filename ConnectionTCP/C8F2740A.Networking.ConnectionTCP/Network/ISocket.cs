using System;
using System.Net;
using System.Threading.Tasks;

namespace C8F2740A.Networking.ConnectionTCP.Network
{
    public interface ISocket : IDisposable
    {
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        bool Connected { get; }
        string Tag { get; }

        void Bind(IPAddress ipAddress, int port);
        void Connect(IPAddress ipAddress, int port);
        void Listen(int backlog);
        void Send(byte[] data);
        int Receive(byte[] bytes);
        Task<ISocket> AcceptAsync();
        void Close();
    }
}