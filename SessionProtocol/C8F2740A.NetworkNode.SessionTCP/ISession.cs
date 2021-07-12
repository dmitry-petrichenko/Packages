using System;
using System.Collections.Generic;

namespace C8F2740A.NetworkNode.SessionTCP
{
    public interface ISession : IDisposable
    {
        void Response(IEnumerable<byte> data);
        void Send(IEnumerable<byte> data);
        void Close();
        void Listen();
        
        event Action<IEnumerable<byte>> Received;
        event Action<IEnumerable<byte>> Responded;
        event Action Closed;
    }
}