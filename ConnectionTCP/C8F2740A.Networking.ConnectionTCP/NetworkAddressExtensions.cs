using System.Text.RegularExpressions;

namespace C8F2740A.Networking.ConnectionTCP
{
    public static class NetworkAddressExtensions
    {
        private static Regex _addressPattern = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}$");
        
        public static bool IsCorrectIPv4Address(this string value)
        {
            return _addressPattern.IsMatch(value);
        }
    }
}