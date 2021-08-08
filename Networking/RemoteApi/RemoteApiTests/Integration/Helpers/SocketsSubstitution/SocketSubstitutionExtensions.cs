using System.Threading.Tasks;

namespace RemoteApi.Integration.Helpers.SocketsSubstitution
{
    public static class SocketSubstitutionExtensions
    {
        public static Task<bool> ArrangeWaiting(
            this SocketSubstitution socketSubstitution,
            Counter parameter, 
            int aimedValue, 
            int timeoutTime = 8000)
        {
            return new SocketSubstitutionStateAwaitor(socketSubstitution, parameter, aimedValue, timeoutTime).Task;
        }
    }
}