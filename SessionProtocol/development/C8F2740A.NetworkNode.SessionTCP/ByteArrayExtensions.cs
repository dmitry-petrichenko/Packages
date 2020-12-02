using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace C8F2740A.NetworkNode.SessionProtocol
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
            return ExtractBytePrefiInternal(rawData.First());;
        }
        
        public static byte ExtractBytePrefix(this byte value)
        {
            return ExtractBytePrefiInternal(value);
        }

        private static byte ExtractBytePrefiInternal(byte firstByte)
        {
            var firstByteVector = new BitVector32(firstByte);
            var firstBytePrefix = BitConverter.GetBytes(firstByteVector[FirstByteSections.PREFIX]).First();
            
            return firstBytePrefix;
        }
    }
}