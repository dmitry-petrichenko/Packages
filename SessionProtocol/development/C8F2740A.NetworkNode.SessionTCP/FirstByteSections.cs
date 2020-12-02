using System.Collections.Specialized;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    internal class FirstByteSections
    {
        public static BitVector32.Section INDEX = BitVector32.CreateSection(15);
        public static BitVector32.Section PREFIX = BitVector32.CreateSection(15, INDEX);
    }
}