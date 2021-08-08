using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace C8F2740A.NetworkNode.SessionTCP
{
    internal static class ByteArrayExtensions
    {
        public static IEnumerable<byte> WrapDataWithFirstByte(this IEnumerable<byte> rawData, byte prefix)
        {
            var rawDataList = rawData.ToList();
            rawDataList.Insert(0, prefix);

            return rawDataList;
        }
        
        public static byte ExtractDataPrefix(this IEnumerable<byte> rawData)
        {
            return ExtractBytePrefixInternal(rawData.First());;
        }
        
        public static byte ExtractDataIndex(this IEnumerable<byte> rawData)
        {
            var firstByteVector = new BitVector32(rawData.First());
            
            var firstBytePrefix = BitConverter.GetBytes(firstByteVector[FirstByteSections.INDEX]).First();
            
            return firstBytePrefix;
        }
        
        public static byte ExtractBytePrefix(this byte value)
        {
            return ExtractBytePrefixInternal(value);
        }

        private static byte ExtractBytePrefixInternal(byte firstByte)
        {
            var firstByteVector = new BitVector32(firstByte);
            
            var firstBytePrefix = BitConverter.GetBytes(firstByteVector[FirstByteSections.PREFIX]).First();
            
            return firstBytePrefix;
        }
        
        public static IEnumerable<byte> ToEnumerable(this byte value)
        {
            var result = Enumerable.Repeat(value, 1);
            
            return result;
        }
    }
}