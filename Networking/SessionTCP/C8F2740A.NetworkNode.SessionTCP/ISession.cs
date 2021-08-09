using System;
using System.Collections.Generic;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface ISession
    {
        void Response(IEnumerable<byte> data);
        void Send(IEnumerable<byte> data);
        void Dispose();
        void Listen();
        
        event Action<IEnumerable<byte>> Received;
        event Action<IEnumerable<byte>> Responded;
        event Action Disconnected;
    }
}