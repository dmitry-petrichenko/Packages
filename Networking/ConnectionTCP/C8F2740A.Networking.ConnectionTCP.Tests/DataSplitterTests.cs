using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets;
using C8F2740A.Networking.ConnectionTCP.Network.Sockets;
using Xunit;

namespace SegmentedSocketTests
{
    public class DataSplitterTests
    {
        private IDataSplitter _sut;
        private SocketMock _socketMock;

        public DataSplitterTests()
        {
            _socketMock = new SocketMock();
            _sut = new DataSplitter(_socketMock);
        }
        
        [Fact]
        public void Receive_WhenCalledWithManyMessages_ShouldReturnByOne()
        {
            List<string> messages = new List<string>();
            messages.Add("hello");
            messages.Add("world");
            ArrangeSocketToReturnSeveralMessages(messages);

            var byteResult1 = _sut.Receive();
            var stringResult1 = Encoding.ASCII.GetString(byteResult1);
            var byteResult2 = _sut.Receive();
            var stringResult2 = Encoding.ASCII.GetString(byteResult2);

            Assert.Equal("hello", stringResult1);
            Assert.Equal("world", stringResult2);
        }
        
        [Fact]
        public void Send_WhenCalled_ShouldWrapMessage()
        {
            byte[] actualBytes = Array.Empty<byte>();
            _socketMock.SendCalled += bytes =>
            {
                actualBytes = bytes;
            };
            
            _sut.Send(Encoding.ASCII.GetBytes("hello"));
            
            var expectedBytes = DataFormatter.WrapWithSeparation(
                Encoding.ASCII.GetBytes("hello"));

            Assert.True(Enumerable.SequenceEqual(expectedBytes, actualBytes));
        }

        private void ArrangeSocketToReturnSeveralMessages(IEnumerable<string> messages)
        {
            byte[] byteMessage;
            byte[] byteResult = Array.Empty<byte>();
            foreach (var message in messages)
            {
                byteMessage = DataFormatter.WrapWithSeparation(
                    Encoding.ASCII.GetBytes(message));

                byteResult = byteResult.Concat(byteMessage).ToArray();
            }

            bool called = false;
            _socketMock.ReceiveCalled += bytes =>
            {
                if (!called)
                {
                    SetBytesToBeginOfArray(bytes, byteResult);
                    return byteResult.Length;
                }

                return 0;
            };
        }

        private void SetBytesToBeginOfArray(byte[] bytes, byte[] bytesToSet)
        {
            for (int i = 0; i < bytesToSet.Length; i++)
            {
                bytes[i] = bytesToSet[i];
            }
        }
        

        private class SocketMock : ISocket
        {
            public event Func<byte[], int> ReceiveCalled;
            public event Action<byte[]> SendCalled;
            
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public IPEndPoint LocalEndPoint { get; }
            public IPEndPoint RemoteEndPoint { get; }
            public bool Connected { get; }
            public string Tag { get; }
            public void Bind(IPAddress ipAddress, int port)
            {
                throw new NotImplementedException();
            }

            public void Connect(IPAddress ipAddress, int port)
            {
                throw new NotImplementedException();
            }

            public void Listen(int backlog)
            {
                throw new NotImplementedException();
            }

            public void Send(byte[] data)
            {
                SendCalled?.Invoke(data);
            }

            public int Receive(byte[] bytes)
            {
                if (ReceiveCalled != null) 
                    return ReceiveCalled.Invoke(bytes);
                return 0;
            }

            public Task<ISocket> AcceptAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
    

}