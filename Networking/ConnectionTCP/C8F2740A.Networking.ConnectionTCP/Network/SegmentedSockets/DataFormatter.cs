using System.Collections.Generic;
using System.Linq;

namespace C8F2740A.Networking.ConnectionTCP.Network.SegmentedSockets
{
    public static class DataFormatter
    {
        internal const byte SEPARATE_FIRST = 0b1000_0000;
        internal const byte SEPARATE_SECOND = 0b0000_0001;
        internal static byte[] SEPARATION =  { SEPARATE_FIRST, SEPARATE_SECOND };
        
        public static byte[][] ExtractFromSeparation(byte[] arr)
        {
            var result = new Dictionary<int, List<byte>>();

            var index = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == SEPARATE_FIRST)
                {
                    if (arr.IsNextValueSeparating(i))
                    {
                        i++;
                        index++;
                        continue;
                    }
                }
                
                result.GetValue(index).Add(arr[i]);
            }

            var toReturn = new List<byte[]>();
            foreach (var resultValue in result.Values)
            {
                toReturn.Add(resultValue.ToArray());
            }

            return toReturn.ToArray();
        }
        
        public static byte[] WrapWithSeparation(byte[] data)
        {
            data = SEPARATION.Concat(data).ToArray();
            
            return data;
        }
    }
    
    internal static class ArrayExtensions
    {
        public static bool IsNextValueSeparating(this byte[] arr , int currentIndex)
        {
            if (currentIndex < arr.Length)
            {
                if (arr[currentIndex + 1] == DataFormatter.SEPARATE_SECOND)
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
    
    internal static class DictionaryExtensions
    {
        public static List<byte> GetValue(this Dictionary<int, List<byte>> dictionary, int key)
        {
            List<byte> _list;

            if (dictionary.TryGetValue(key, out _list))
            {
                return _list;
            }
            
            _list = new List<byte>();
            dictionary.Add(key, _list);

            return _list;
        }
    }
}