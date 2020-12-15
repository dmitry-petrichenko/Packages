using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionTCP;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class SessionIntegrativeTests
    {
        [Fact]
        public void TwoSessions_WhenInteract_ShouldExchangeDataCorrectly()
        {
            var rawDataToSend = BitConverter.GetBytes(644);
            var rawDataToResponse = BitConverter.GetBytes(1121);
            var actualReceivedData = default(IEnumerable<byte>);
            var actualResponded = default(IEnumerable<byte>);
            
            var recorder = Mock.Create<IRecorder>();
            var networkTunnelSession1 = new NetworkTunnelMock();
            var networkTunnelSession2 = new NetworkTunnelMock();
            networkTunnelSession1.RemoteTunnel = networkTunnelSession2;
            networkTunnelSession2.RemoteTunnel = networkTunnelSession1;
            var session1 = CreateSession(networkTunnelSession1, recorder);
            var session2 = CreateSession(networkTunnelSession2, recorder);
            session1.Responded += Responded;
            session2.Received += Received;
            
            void Responded(IEnumerable<byte> bytes) => actualResponded = bytes;

            void Received(IEnumerable<byte> bytes) => actualReceivedData = bytes;

            session1.Send(rawDataToSend);
            session2.Response(rawDataToResponse);

            Assert.Equal(rawDataToSend, actualReceivedData);
            Assert.Equal(rawDataToResponse, actualResponded);
        }
        
        [Fact]
        public void TwoSessions_WhenInteractManyTimes_ShouldExchangeDataCorrectly()
        {
            var session1ToSend = default(IEnumerable<byte>);//BitConverter.GetBytes(644);
            var session2ToSend = default(IEnumerable<byte>);//BitConverter.GetBytes(111);
            
            var session1ActualReceived = default(IEnumerable<byte>);
            var session2ActualReceived = default(IEnumerable<byte>);
            
            var session1ToResponse = default(IEnumerable<byte>);// = BitConverter.GetBytes(1121);
            var session2ToResponse = default(IEnumerable<byte>);// = BitConverter.GetBytes(40);
            
            var session1ActualRepsonse = default(IEnumerable<byte>);
            var session2ActualRepsonse = default(IEnumerable<byte>);

            var recorder = Mock.Create<IRecorder>();
            var networkTunnelSession1 = new NetworkTunnelMock();
            var networkTunnelSession2 = new NetworkTunnelMock();
            networkTunnelSession1.RemoteTunnel = networkTunnelSession2;
            networkTunnelSession2.RemoteTunnel = networkTunnelSession1;
            var session1 = CreateSession(networkTunnelSession1, recorder);
            var session2 = CreateSession(networkTunnelSession2, recorder);
            session1.Responded += Session1Responded;
            session2.Responded += Session2Responded;
            session1.Received += Session1Received;
            session2.Received += Session2Received;
            
            void Session1Responded(IEnumerable<byte> bytes) => session1ActualRepsonse = bytes;
            void Session2Responded(IEnumerable<byte> bytes) => session2ActualRepsonse = bytes;
            void Session1Received(IEnumerable<byte> bytes) => session1ActualReceived = bytes;
            void Session2Received(IEnumerable<byte> bytes) => session2ActualReceived = bytes;

            for (int i = 0; i < 30; i++)
            {
                session1ToSend = BitConverter.GetBytes(i * 5);
                session2ToSend = BitConverter.GetBytes(i + 2);
                
                session1ToResponse = BitConverter.GetBytes(i + 100);
                session2ToResponse = BitConverter.GetBytes(i + 200);
                
                session1.Send(session1ToSend);
                session2.Send(session2ToSend);
                session1.Response(session1ToResponse);
                session2.Response(session2ToResponse);
            
                Assert.Equal(session1ToSend, session2ActualReceived);
                Assert.Equal(session2ToSend, session1ActualReceived);
                Assert.Equal(session1ToResponse, session2ActualRepsonse);
                Assert.Equal(session2ToResponse, session1ActualRepsonse);
            }

            Mock.Assert(() => recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Never());
        }
        
        private ISession CreateSession(INetworkTunnel networkTunnel, IRecorder recorder)
        {
            return new Session(networkTunnel, recorder);
        }
        
        private class NetworkTunnelMock : INetworkTunnel
        {
            public void Dispose() { }

            public Task Listen() => Task.CompletedTask;
            
            public void Send(byte[] data)
            {
                RemoteTunnel?.Received?.Invoke(data);
            }

            public void Close()
            {
                
            }

            public NetworkTunnelMock RemoteTunnel { get; set; }

            public event Action<byte[]> Received;
            public event Action Closed;
        }
    }
}