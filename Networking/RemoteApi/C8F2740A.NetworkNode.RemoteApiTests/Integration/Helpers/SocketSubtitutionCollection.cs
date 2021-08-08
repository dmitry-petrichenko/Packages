using System;
using System.Collections.Concurrent;
using RemoteApi.Integration.Helpers.SocketsSubstitution;

namespace RemoteApi.Integration.Helpers
{
    public class SocketSubtitutionCollection : ConcurrentBag<SocketSubstitution>
    {
        public event Action<SocketSubstitution> SocketAdded;
        
        public void AddSocket(SocketSubstitution socket)
        {
            Add(socket);
            SocketAdded?.Invoke(socket);
        }
    }
}