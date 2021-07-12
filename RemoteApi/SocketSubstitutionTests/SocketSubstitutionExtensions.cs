using System;
using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;

namespace SocketSubstitutionTests
{
    public static class SocketSubstitutionExtensions
    {
        public static Task<bool> ArrangeWaiting(
            this SocketSubstitution socketSubstitution,
            Counter parameter, 
            int aimedValue, 
            int timeoutTime = 1000)
        {
            return new SocketSubstitutionStateAwaitor(socketSubstitution, parameter, aimedValue, timeoutTime).Task;
        }
    }
}