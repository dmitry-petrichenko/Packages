using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets
{
    public class DataSplitterFactory : IDataSplitterFactory
    {
        public IDataSplitter Create(ISocket socket)
        {
            return new DataSplitter(socket);
        }
    }

    public interface IDataSplitterFactory
    {
        IDataSplitter Create(ISocket socket);
    }
}