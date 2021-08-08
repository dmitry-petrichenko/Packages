using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;

namespace C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets
{
    public interface IDataSplitter
    {
        byte[] Receive();
        void Send(byte[] data);
        void Dispose();
    }
    
    public class DataSplitter : IDataSplitter
    {
        private readonly ISocket _socket;
        
        private Queue<byte[]> _buffer;

        public DataSplitter(ISocket socket)
        {
            _socket = socket;
            _buffer = new Queue<byte[]>();
        }

        public byte[] Receive()
        {
            if (_buffer.Count > 0)
            {
                var msg = _buffer.Dequeue();
                return msg;
            }

            byte[] data = new byte[1024];
            int bytes = _socket.Receive(data);

            if (bytes == 0)
            {
                return Array.Empty<byte>();
            }

            var msgs = DataFormatter.ExtractFromSeparation(data.Take(bytes).ToArray());
            foreach (var msg in msgs)
            {
                _buffer.Enqueue(msg);
            }
            
            var message = _buffer.Dequeue();
            return message;
        }

        public void Send(byte[] data)
        {
            _socket.Send(DataFormatter.WrapWithSeparation(data));
        }

        public void Dispose()
        {
            _buffer.Clear();
        }
    }

}