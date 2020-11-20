using System.Collections.Specialized;

namespace C8F2740A.NetworkNode.Commands
{
    public static class DataSections
    {
        public static BitVector32.Section STATUS = BitVector32.CreateSection(1); //1
        public static BitVector32.Section LIGHT = BitVector32.CreateSection(1, STATUS); //1
        public static BitVector32.Section HUMIDITY = BitVector32.CreateSection(1, LIGHT); //1
        public static BitVector32.Section FLOODING = BitVector32.CreateSection(1, HUMIDITY); //1
        public static BitVector32.Section WATER_STOCK_LEVEL = BitVector32.CreateSection(1, FLOODING); //1
        public static BitVector32.Section DATE_DAY = BitVector32.CreateSection(31, WATER_STOCK_LEVEL); //5
        public static BitVector32.Section DATE_MONTH = BitVector32.CreateSection(15, DATE_DAY); //4
        public static BitVector32.Section DATE_YEAR = BitVector32.CreateSection(31, DATE_MONTH); //5
        public static BitVector32.Section DATE_HOURS = BitVector32.CreateSection(31, DATE_YEAR); //5
        public static BitVector32.Section DATE_MINUTES = BitVector32.CreateSection(61, DATE_HOURS); //6
    }
}