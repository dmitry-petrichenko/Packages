using System.Text.RegularExpressions;

namespace C8F2740A.Networking.NetworkExtensions
{
    public static class AddressIPv4Extensions
    {
        public static bool IsCorrectIPv4Address(this string address)
        {
            Regex addressPatern = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\:\d{1,5}$");

            return addressPatern.IsMatch(address);
        }
    }
}