using System.Threading.Tasks;
using RemoteApi.Integration.Helpers;

namespace SocketSubstitutionTests
{
    public static class SocketSubstitutionExtensions
    {
        public static Task Arrange(
            this SocketSubstitution socketSubstitution,
            Counter parameter, 
            int aimedValue)
        {
            return new SocketSubstitutionStateAwaitor(socketSubstitution, parameter, aimedValue).Task;
        }
    }
}