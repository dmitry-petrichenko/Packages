using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteApi
{
    public static class CommandExtensions
    {
        public static IEnumerable<byte> ToEnumerableByte(this String value)
        {
            return Encoding.ASCII.GetBytes(value);
        }
        
        public static string ToText(this IEnumerable<byte> value)
        {
            return Encoding.ASCII.GetString(value.ToArray());
        }
    }
}