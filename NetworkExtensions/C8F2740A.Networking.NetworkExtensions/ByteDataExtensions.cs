using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C8F2740A.Networking.NetworkExtensions
{
    public static class ByteDataExtensions
    {
        public static byte[] ToBytesArray(this byte command)
        {
            return new[] { command };
        }
        
        public static string ToText(this IEnumerable<byte> bytes)
        {
            var arr = bytes.ToArray();
            return Encoding.UTF8.GetString(arr, 0, arr.Length);
        }
    }
}