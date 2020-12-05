using System;
using System.Collections.Generic;
using System.Linq;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionProtocol;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class SessionTests
    {
        private ISession _sut;
        private INetworkTunnel _networkTunnel;
        private IRecorder _recorder;
        
        public SessionTests()
        {
            _networkTunnel = Mock.Create<INetworkTunnel>();
            _recorder = Mock.Create<IRecorder>();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldStartListen()
        {
            _sut = new Session(_networkTunnel, _recorder);
            
            Mock.Assert(() => _networkTunnel.Listen(), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribeOnClose()
        {
            _networkTunnel.ArrangeSet(x => x.Closed += null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _networkTunnel.AssertAll();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribeOnReceived()
        {
            _networkTunnel.ArrangeSet(x => x.Received += null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _networkTunnel.AssertAll();
        }
        
        [Fact]
        public void Dispose_WhenCalled_ShouldSubscribeFromSubscriptions()
        {
            _networkTunnel.ArrangeSet(x => x.Received -= null).IgnoreArguments().Occurs(1);
            _networkTunnel.ArrangeSet(x => x.Closed -= null).IgnoreArguments().Occurs(1);
            _sut = new Session(_networkTunnel, _recorder);
            
            _sut.Dispose();;
            
            _networkTunnel.AssertAll();
        }
        
        [Fact]
        public void RequestInnerCall_WhenCalled_ShouldReceiveData()
        {
            var actualReceived = default(IEnumerable<byte>);
            var data = BitConverter.GetBytes(23443223);
            var dataWithPrefix = GenerateDataWithPrefix(Session.REQUEST, data, false);
            _sut = new Session(_networkTunnel, _recorder);
            _sut.Received += received => actualReceived = received;

            Mock.Raise(() => _networkTunnel.Received += null, dataWithPrefix.ToArray()); 

            Assert.Equal(data, actualReceived);
        }
        
        [Fact]
        public void RequestInnerCall_WhenCalledWithEmptyData_ShouldReceiveEmptyArray()
        {
            var actualReceived = default(IEnumerable<byte>);
            var data = Array.Empty<byte>();
            var dataWithPrefix = GenerateDataWithPrefix(Session.REQUEST, data, false);
            _sut = new Session(_networkTunnel, _recorder);
            _sut.Received += received => actualReceived = received;

            Mock.Raise(() => _networkTunnel.Received += null, dataWithPrefix.ToArray()); 

            Assert.Equal(data, actualReceived);
        }
        
        [Fact]
        public void ResponseInnerCall_WhenCalledFirst_ShouldNotReceiveData()
        {
            var actualReceived = default(IEnumerable<byte>);
            var data = BitConverter.GetBytes(23443223);
            var dataWithPrefix = GenerateDataWithPrefix(Session.RESPONSE, data);
            _sut = new Session(_networkTunnel, _recorder);
            _sut.Received += received => actualReceived = received;
            
            Mock.Raise(() => _networkTunnel.Received += null, dataWithPrefix.ToArray()); 

            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
            Assert.Equal(default(IEnumerable<byte>), actualReceived);
        }
        
        [Theory]
        [InlineData(13)]
        [InlineData(3455)]
        [InlineData(0)]
        public void ResponseInnerCall_WhenCalledAfterRequest_ShouldReceiveData(int dataToTransfer)
        {
            var actualResponded = default(IEnumerable<byte>);
            var data1 = BitConverter.GetBytes(0);
            var data2 = BitConverter.GetBytes(dataToTransfer);
            var requestDataWithPrefix = GenerateDataWithPrefix(Session.REQUEST, data1);
            var responseDataWithPrefix = GenerateDataWithPrefix(Session.RESPONSE, data2, true);
            _sut = new Session(_networkTunnel, _recorder);
            Mock.Raise(() => _networkTunnel.Received += null, requestDataWithPrefix.ToArray());
            Mock.Arrange(() => _networkTunnel.Send(Arg.IsAny<byte[]>())).DoInstead(
                (byte[] args) => actualResponded = args);
            
            _sut.Response(BitConverter.GetBytes(dataToTransfer));
            
            Assert.Equal(responseDataWithPrefix, actualResponded);
        }
        
        [Fact]
        public void ResponseInnerCall_WhenCalledBeforeRequest_ShouldNotReceiveData()
        {
            var response = 13;
            var actualResponded = default(IEnumerable<byte>);
            _sut = new Session(_networkTunnel, _recorder);
            _sut.Responded += responded => actualResponded = responded;
            Mock.Arrange(() => _networkTunnel.Send(Arg.IsAny<byte[]>())).DoInstead(
                (byte[] args) => actualResponded = args);
            
            _sut.Response(BitConverter.GetBytes(response));
            
            Mock.Assert(() => _recorder.RecordError(Arg.AnyString, Arg.AnyString), Occurs.Once());
            Assert.Equal(default, actualResponded);
        }

        private byte[] GenerateDataWithPrefix(byte prefix, byte[] data, bool responseBehaviour = false, int startIndex = int.MinValue)
        {
            var indexerCalculator = default(IndexerCalculator);
            if (startIndex == int.MinValue)
            {
                indexerCalculator = new IndexerCalculator(responseBehaviour);
            }
            else
            {
                indexerCalculator = new IndexerCalculator(responseBehaviour, startIndex);
            }

            var nextPrefix = indexerCalculator.GenerateIndexToSend(prefix);;
            var dataWithPrefix = data.WrapDataWithFirstByte(nextPrefix);

            return dataWithPrefix.ToArray();
        }
    }
}