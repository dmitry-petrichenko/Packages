using System;
using System.Text;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.Commands;
using Telerik.JustMock;
using Xunit;

namespace C8F2740A.NetworkNode.SessionProtocol.Tests
{
    public class ReceiveSessionTests
    {
        private IReceiveSession _sut;
        private INetworkTunnel _networkTunnel;
        private IRecorder _recorder;
        
        public ReceiveSessionTests()
        {
            _networkTunnel = Mock.Create<INetworkTunnel>();
            _recorder = Mock.Create<IRecorder>();
            
            _sut = new ReceiveSession(_networkTunnel, _recorder);
        }

        [Fact]
        public void CloseEvent_OnCloseTunnel_ShouldRaise()
        {
            var closeRaised = false;
            _sut.Closed += () => closeRaised = true;
            
            Mock.Raise(() => _networkTunnel.Closed += null);
            
            Assert.True(closeRaised);
        }
        
        [Fact]
        public void Dispose_OnCall_ShouldDisposeNetworkTunnel()
        {
            _sut.Dispose();

            Mock.Assert(() => _networkTunnel.Dispose(), Occurs.Once());
        }
        
        [Fact]
        public void Dispose_OnCall_ShouldUnsubcribeFromNetworkTunnelReceived()
        {
            // Arrange
            var closeRaised = false;
            _sut.Dispose();
            _sut.Closed += () => closeRaised = true;
            
            // Act 
            Mock.Raise(() => _networkTunnel.Closed += null);
            
            // Assert
            Assert.False(closeRaised);
        }
        
        [Fact]
        public void Validate_OnCloseTunnel_ShouldRaise()
        {
            var closeRaised = false;
            _sut.Closed += () => closeRaised = true;
            
            Mock.Raise(() => _networkTunnel.Closed += null);
            
            Assert.True(closeRaised);
        }
        
        [Fact]
        public void Validate_OnReceivedNotSixBytes_ReturnsFalse()
        {
            //Arrange 
            var resultTask = _sut.Validate();
            
            // Act 
            Mock.Raise(() => _networkTunnel.Received += null, new byte[]
            {
                0b00000000, 0b00000000, 0b00000000,
                0b00000000, 0b00000000, 0b00000000
            });
            
            //Assert 
            var result = resultTask.Result;
            Assert.False(result);
        }
        
        [Fact]
        public void Validate_OnReceivedNotRequestByte_ReturnsFalse()
        {
            //Arrange 
            var resultTask = _sut.Validate();
            
            // Act 
            Mock.Raise(() => _networkTunnel.Received += null, new byte[] { 0b00000000 });
            
            //Assert 
            var result = resultTask.Result;
            Assert.False(result);
        }

        [Fact]
        public async Task Validate_OnReceivedCorrectLogin_SendsReceivedBytes2()
        {
            var receivedFromSut = Array.Empty<byte>();
            _sut.Received += bytes => receivedFromSut = bytes;
            var receivedBytes = Encoding.ASCII.GetBytes("aninel");

            Mock.Raise(() => _networkTunnel.Received += null, receivedBytes );
            Mock.Raise(() => _networkTunnel.Received += null, new [] { (byte)NodeCommands.COMMAND_GET } );
            await Task.Delay(100);
            Assert.Equal(new [] { (byte)NodeCommands.COMMAND_GET }, receivedFromSut);
        }

        [Theory]
        [InlineData("aninul")]
        [InlineData("")]
        [InlineData("dima")]
        [InlineData("admin")]
        public void Validate_OnReceivedCorrectLogin_SendsLoginFail(string login)
        {
            //Arrange 
            var resultTask = _sut.Validate();
            
            // Act 
            Mock.Raise(() => _networkTunnel.Received += null, Encoding.ASCII.GetBytes(login) );
            
            //Assert 
            Mock.Assert(() => _networkTunnel.Send(new byte[] { (byte)NodeCommands.RESPONSE_LOGIN_FAIL }), Occurs.Once());
            Mock.Assert(() => _networkTunnel.Send(new byte[] { (byte)NodeCommands.RESPONSE_LOGIN_SUCCESS }), Occurs.Never());
            var result = resultTask.Result;
            Assert.False(result);
        }
        
        [Fact]
        public void BytesReceivedHandler_OnReceivedCorrectCommand_RaiseInReceived()
        {
            // Arrange 
            var bytesReceived = default(byte[]);
            _sut.Received += b => bytesReceived = b; 
            
            // Act 
            Mock.Raise(() => _networkTunnel.Received += null, Encoding.ASCII.GetBytes("aninel") );
            Mock.Raise(() => _networkTunnel.Received += null, new byte[] { (byte)NodeCommands.COMMAND_GET });
            
            //Assert 
            Mock.Assert(() => _networkTunnel.Send(new byte[] { (byte)NodeCommands.RESPONSE_COMMAND_RECEIVED }), Occurs.Once());
            Assert.Equal(bytesReceived[0], (byte)NodeCommands.COMMAND_GET);
        }
        
        [Fact]
        public void BytesReceivedHandler_OnReceivedIncorrectCommand_SendsWrongCommandNotRaiseReceive()
        {
            // Arrange 
            var raised = false;
            _sut.Received += b => raised = true; 
            
            // Act 
            Mock.Raise(() => _networkTunnel.Received += null, Encoding.ASCII.GetBytes("aninel") );
            Mock.Raise(() => _networkTunnel.Received += null, new byte[] { 0b01110000 });
            
            //Assert 
            Mock.Assert(() => _networkTunnel.Send(new byte[] { (byte)NodeCommands.RESPONSE_COMMAND_WRONG }), Occurs.Once());
            Assert.False(raised);
        }
    }
}