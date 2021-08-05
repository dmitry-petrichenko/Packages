using System;
using System.Collections.Generic;

namespace RemoteApi.Integration.Helpers
{
    public class SocketSubtitutionCollection : List<SocketsSubstitution.SocketSubstitution>
    {
        public event Action<SocketsSubstitution.SocketSubstitution> SocketAdded;
        
        public void AddSocket(SocketsSubstitution.SocketSubstitution socket)
        {
            Add(socket);
            SocketAdded?.Invoke(socket);
        }
    }
}