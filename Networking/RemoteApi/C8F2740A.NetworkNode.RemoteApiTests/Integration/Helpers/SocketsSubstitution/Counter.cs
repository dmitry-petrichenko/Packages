using System.Threading;

namespace RemoteApi.Integration.Helpers.SocketsSubstitution
{
    public class Counter
    {
        private int _value;

        public void Tick()
        {
            Interlocked.Increment(ref _value);
        }

        public int Value => _value;
    }
}