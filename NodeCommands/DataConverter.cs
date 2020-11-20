using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;

namespace C8F2740A.NetworkNode.Commands
{
    public interface IDataConverter
    {
        string ConvertStatusResponse(byte[] data);
    }
    
    public class DataConverter : IDataConverter
    {
        private Dictionary<NodeCommands, string> _commandsToStringMap;

        public DataConverter()
        {
        }

        public string ConvertStatusResponse(byte[] data)
        {
            if (data.Length > 4)
            {
                throw new Exception("");
            }
            
            var vector = new BitVector32(BitConverter.ToInt32(data));

            var result =
                $"Status: {IntToBoolStatus(vector[DataSections.STATUS])} " +
                $"\nLight: {IntToBoolStatus(vector[DataSections.LIGHT])}" +
                $"\nHumidity: {IntToBoolStatus(vector[DataSections.HUMIDITY])}" +
                $"\nFlooding: {IntToBoolStatus(vector[DataSections.FLOODING])} " +
                $"\nWater stock level: {IntToBoolStatus(vector[DataSections.WATER_STOCK_LEVEL])}" +
                $"\n20{DigitToDecimalConvert(vector[DataSections.DATE_YEAR])}." +
                $"{DigitToDecimalConvert(vector[DataSections.DATE_MONTH])}." +
                $"{DigitToDecimalConvert(vector[DataSections.DATE_DAY])}|" +
                $"{DigitToDecimalConvert(vector[DataSections.DATE_HOURS])}:" +
                $"{DigitToDecimalConvert(vector[DataSections.DATE_MINUTES])}";

            return result;
        }
        
        private string DigitToDecimalConvert(int value)
        {
            if (value < 10)
            {
                return $"0{value}";
            }

            return value.ToString();
        }

        private string IntToBoolStatus(int value)
        {
            return value != 0 ? "OK" : "FAIL";
        }
    }
}