using System;
using System.Collections.Generic;

namespace RemoteApi.Integration.Helpers
{
    public class SocketSubtitutionCollection : List<SocketSubstitution>
    {
        public event Action<SocketSubstitution> SocketAdded;
        
        public void AddSocket(SocketSubstitution socket)
        {
            Add(socket);
            SocketAdded?.Invoke(socket);
        }
    }
}