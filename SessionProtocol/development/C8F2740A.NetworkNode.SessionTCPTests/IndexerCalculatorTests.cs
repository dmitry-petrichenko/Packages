using System;
using C8F2740A.NetworkNode.SessionProtocol;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class IndexerCalculatorTests
    {
        private IndexerCalculator _sut;
        
        [Theory]
        [InlineData(0b1110_1111, true, 14)]
        [InlineData(0b1110_0000, true, 15)]
        [InlineData(0b1110_0001, true, 16)]
        [InlineData(0b1110_1111, false, 14)]
        [InlineData(0b1110_0000, false, 15)]
        [InlineData(0b1110_0001, false, 16)]
        public void GenerateIndexToSend_WhenCalledForRequestBehaviourOn15_ShouldSend0(byte data, bool isResponseBehaviour, int indexValue)
        {
            _sut = new IndexerCalculator(isResponseBehaviour);
            byte firstByte = 0b1110_0000;
            for (int i = 0; i++ < indexValue;) _sut.GenerateIndexToSend(firstByte);

            var result = _sut.GenerateIndexToSend(firstByte);
            
            Assert.Equal(data, result);
        }

        [Fact]
        public void GenerateIndexToSend_WhenCalledForRequestBehaviour_ShouldSendNextValue()
        {
            _sut = new IndexerCalculator(false);
            byte firstByte = 0b0101_0000;

            var result = _sut.GenerateIndexToSend(firstByte);
            
            Assert.Equal(0b0101_0001, result);
        }
        
        [Fact]
        public void GenerateIndexToSend_WhenCalledForRequestAfterValidate_ShouldSendSameValue()
        {
            _sut = new IndexerCalculator(false);
            byte firstByte = 0b1111_0000;
            var validationResult = _sut.ValidateCurrentIndex(firstByte);
            
            var result = _sut.GenerateIndexToSend(firstByte);
            
            Assert.True(validationResult);
            Assert.Equal(0b1111_0001, result);
        }
        
        [Theory]
        [InlineData(0b1111_0001, true)]
        [InlineData(0b1111_0000, false)]
        public void ValidateCurrentIndex_WhenCalledForRequestAfterGenerateIndexToSend_ShouldSendNextValue(byte data, bool expectedResult)
        {
            _sut = new IndexerCalculator(false);
            _sut.GenerateIndexToSend(0b1000_0000);
            
            var validationResult = _sut.ValidateCurrentIndex(data);

            Assert.Equal(expectedResult, validationResult);
        }

        [Fact]
        public void GenerateIndexToSend_WhenCalledForResponseBehaviour_ShouldSendSameValue()
        {
            _sut = new IndexerCalculator(true);
            
            var result = _sut.GenerateIndexToSend(0b0101_0000);
            
            Assert.Equal(0b0101_0001, result);
        }
        
        [Fact]
        public void GenerateIndexToSend_WhenCalledForResponseAfterValidate_ShouldSendSameValue()
        {
            _sut = new IndexerCalculator(true);
            var validationResult = _sut.ValidateCurrentIndex(0b1111_0001);
            
            var result = _sut.GenerateIndexToSend(0b1111_1111);
            
            Assert.True(validationResult);
            Assert.Equal(0b1111_0001, result);
        }
        
        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(1, 1, false)]
        [InlineData(1, 2, true)]
        [InlineData(2, 3, true)]
        [InlineData(8, 9, true)]
        [InlineData(15, 0, true)]
        public void GenerateIndexToSend_WhenCalledForRequest_ShouldBeValidOnResponce(
            int requestCurrentIndex, 
            int responseCurrentIndex,
            bool expectedResult)
        {
            var requestCalculator = new IndexerCalculator(false, requestCurrentIndex);
            var responseCalculator = new IndexerCalculator(true, responseCurrentIndex);
            byte firstByte = Session.REQUEST;
            var indexToSend = requestCalculator.GenerateIndexToSend(firstByte);
            var temp = ByteToString(indexToSend);
            var result = responseCalculator.ValidateCurrentIndex(indexToSend);
            
            Assert.Equal(expectedResult, result);
        }
        
        [Fact]
        public void ValidateCurrentIndex_WhenCalledForResponseWithWrongValue_ShouldReturnFalse()
        {
            _sut = new IndexerCalculator(true);
            byte firstByte = 0b1111_0000;
            var validationResult = _sut.ValidateCurrentIndex(firstByte);

            Assert.False(validationResult);
        }
        
        [Theory]
        [InlineData(0b1111_0010, true)]
        [InlineData(0b1111_0000, false)]
        public void ValidateCurrentIndex_WhenCalledForResponseAfterGenerateIndexToSend_ShouldSendNextValue(byte data, bool expectedResult)
        {
            _sut = new IndexerCalculator(true);
            _sut.GenerateIndexToSend(0b1000_0000);
            
            var validationResult = _sut.ValidateCurrentIndex(data);

            Assert.Equal(expectedResult, validationResult);
        }

        public static string ByteToString(byte value)
        {
            return Convert.ToString(value, 2).PadLeft(8, '0');
        }
    }
}