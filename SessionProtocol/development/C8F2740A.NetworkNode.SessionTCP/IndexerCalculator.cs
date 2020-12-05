using System;
using System.Collections.Specialized;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    internal class IndexerCalculator
    {
        private int _currentIndex;
        private bool _isResponseBehaviour;
        
        public IndexerCalculator(bool isResponseBehaviour)
        {
            _isResponseBehaviour = isResponseBehaviour;

            _currentIndex = _isResponseBehaviour ? 1 : 0;
        }
        
        internal IndexerCalculator(bool isResponseBehaviour, int currentIndex)
        {
            _currentIndex = currentIndex;
            _isResponseBehaviour = isResponseBehaviour;
        }

        public bool ValidateCurrentIndex(byte firstByte)
        {
            var index = ExtractIndex(firstByte);
            return _currentIndex.Equals(index);
        }

        public byte GenerateIndexToSend(byte prefix)
        {
            var result = default(byte);
            
            if (_isResponseBehaviour)
            {
                result = GenerateFirstByteWithIndex(_currentIndex, prefix);
                _currentIndex++;
            }
            else
            {
                _currentIndex++;
                result = GenerateFirstByteWithIndex(_currentIndex, prefix);
            }
            
            ResetCurrentIndexIfNeeded();

            return result;
        }

        private void ResetCurrentIndexIfNeeded()
        {
            if (_currentIndex >= 16)
            {
                _currentIndex = 0;
            }
        }

        private int ExtractIndex(byte firstByte)
        {
            var rawDataVector = new BitVector32(firstByte);
            var index = rawDataVector[FirstByteSections.INDEX];

            return index;
        }

        private byte GenerateFirstByteWithIndex(int index, byte prefix)
        {
            var indexVector = new BitVector32(index);
            var prefixVector = new BitVector32(prefix);
            prefixVector[FirstByteSections.INDEX] = indexVector[FirstByteSections.INDEX];
            
            return BitConverter.GetBytes(prefixVector.Data)[0];
        }
    }
}