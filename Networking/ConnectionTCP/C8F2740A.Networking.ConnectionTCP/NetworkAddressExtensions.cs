using System;
using System.Text.RegularExpressions;

namespace C8F2740A.Networking.ConnectionTCP
{
    public static class NetworkAddressExtensions
    {
        private static Regex _addressPattern = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}$");
        
        public static bool IsCorrectIPv4Address(this string value)
        {
            if (!_addressPattern.IsMatch(value))
            {
                return false;
            }
            
            string[] addressAndPort= value.Split(":");
            var octets = addressAndPort[0].Split(".");

            foreach (var octet in octets)
            {
                if (!IsValueLessThanByte(octet))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValueLessThanByte(string value)
        {
            var intValue = Convert.ToInt32(value);

            return intValue < 256;
        }
    }
}