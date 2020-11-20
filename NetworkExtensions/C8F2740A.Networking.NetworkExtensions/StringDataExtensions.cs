using System.Text;

namespace C8F2740A.Networking.NetworkExtensions
{
    public static class StringDataExtensions
    {
        public static byte[] ToBytesArray(this string command)
        {
            return Encoding.UTF8.GetBytes(command);
        }
    }
}