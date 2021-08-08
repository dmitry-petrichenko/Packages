using System;
using System.Net;
using Xunit;

namespace C8F2740A.Networking.ConnectionTCP.Tests
{
    public class NetworkAddressTests
    {
        private INetworkAddress _sut;
        
        public NetworkAddressTests()
        {

        }

        [Fact]
        public void Initialize_OnWrongAddress_ThrowsException()
        {
            var exceptionThrown = false;
            try
            {
                new NetworkAddress("123.32131.22");
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
            }
            
            Assert.True(exceptionThrown);
        }
        
        [Fact]
        public void Initialize_OnCorrectAddress_NotThrowsException()
        {
            var exceptionThrown = false;
            try
            {
                new NetworkAddress("123.123.123.123:7654");
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
            }
            
            Assert.False(exceptionThrown);
        }
        
        [Fact]
        public void Initialize_OnCorrectAddress_ReturnParsedPort()
        {
            var exceptionThrown = false;
            var address = new NetworkAddress("123.123.123.123:7654");
            
            Assert.Equal(address.Port, 7654);
        }
        
        [Fact]
        public void Initialize_OnCorrectAddress_ReturnParsedAddress()
        {
            var exceptionThrown = false;
            var address = new NetworkAddress("123.123.123.123:7654");

            Assert.Equal(address.IP, IPAddress.Parse("123.123.123.123"));
        }
    }
}