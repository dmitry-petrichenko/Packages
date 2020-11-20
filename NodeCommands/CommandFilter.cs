using System;
using System.Collections.Specialized;
using System.Linq;

namespace C8F2740A.NetworkNode.Commands
{
    public class CommandFilter
    {
        public static bool IsCommandBelongToPrefix(NodeCommands command, NodeCommands prefix)
        {
            var commandVector = new BitVector32((byte)command);
            var prefixVector = new BitVector32((byte)prefix);
            BitVector32.Section section1 = BitVector32.CreateSection(15);
            BitVector32.Section section2 = BitVector32.CreateSection(15, section1);

            return commandVector[section2].Equals(prefixVector[section2]);
        }
        
        public static byte[] WrapDataWithPrefix(byte[] rawData, byte prefix)
        {
            var rawDataList = rawData.ToList();
            rawDataList.Insert(0, prefix);
            var wrappedData = rawDataList.ToArray();

            return wrappedData;
        }

        public static (bool, byte[]) TryGetInnerData(byte[] rawData, byte prefix)
        {
            var rawDataVector = new BitVector32(rawData[0]);
            var prefixVector = new BitVector32(prefix);
            
            BitVector32.Section section1 = BitVector32.CreateSection(255);

            if (!rawDataVector[section1].Equals(prefixVector[section1]))
            {
                return (false, default);
            }

            var result = rawData.Skip(1).ToArray();
            
            return (true, result);
        }
        
    }
}