using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using SegmentedSocketTests.Helpers;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests.Integration
{
    [Collection("Serial")]
    public class NetworkTunnelTests
    {
        [Theory]
        [InlineData("127.0.0.1:8888", 1, 2, 3, 4, 100, 300)]
        [InlineData("127.0.0.1:8889", 6, 8, 255, 11, 50, 500)]
        public async void NetworkTunnel_WhenReceive_ShouldShouldREceiveSegmented(string address, int i1, int i2, int i3, int i4,
            int sendDelay, int handleReceiveDelay)
        {
            var bytesToSend = IntToArrayByte(i1, i2, i3, i4);
            var actualReceived = new List<byte>();
            int receivedTimes = 0;
            
            (INetworkTunnel accepted, INetworkTunnel connected) 
                = await NetworkTunnelIntegrationHelper.ArrangeNetworkTunnelTwoSides(address, new MockRecorder());
            
            // Arrange receiving
            var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);
            var listenTask = Task.Factory.StartNew(() =>
            {
                connected.Listen();
                connected.Received += bytes =>
                {
                    actualReceived.Add(bytes.FirstOrDefault());
                    Thread.Sleep(handleReceiveDelay); 
                };
                while (actualReceived.Count < bytesToSend.Length) Thread.Sleep(200);

            }, cts.Token);

            // Arrange sending
            var sendTask = Task.Run(() =>
            {
                foreach (var b in bytesToSend)
                {
                    Task.Delay(sendDelay);
                    accepted.Send(new []{ b });
                }
            });

            await Task.WhenAll(listenTask, sendTask);

            // Assert
            Assert.True(Enumerable.SequenceEqual(bytesToSend, actualReceived.ToArray()));
        }

        private byte[] IntToArrayByte(int i1, int i2, int i3, int i4)
        {
            var bytesList = new List<byte>();
            bytesList.Add(Convert.ToByte(i1));
            bytesList.Add(Convert.ToByte(i2));
            bytesList.Add(Convert.ToByte(i3));
            bytesList.Add(Convert.ToByte(i4));

            return bytesList.ToArray();
        }
    }

    internal class MockRecorder : IRecorder
    {
        public void RecordInfo(string tag, string message)
        {
        }

        public void RecordError(string tag, string message)
        {
        }

        public void DefaultException(object source, Exception exception)
        {
        }
    }
}