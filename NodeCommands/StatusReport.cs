using System;
using System.Collections.Specialized;

namespace C8F2740A.NetworkNode.Commands
{
    public class StatusReport
    {
        private readonly DateTime _dateTime;
        private readonly bool _humidity;
        private readonly bool _light;
        private readonly bool _flooding;
        private readonly bool _waterStock;
        private readonly bool _status;

        private BitVector32 _vector;

        public byte[] Data { get; private set; }

        public StatusReport(DateTime dateTime, bool humidity, bool light, bool flooding, bool waterStock, bool status)
        {
            _dateTime = dateTime;
            _humidity = humidity;
            _light = light;
            _flooding = flooding;
            _waterStock = waterStock;
            _status = status;

            ConvertData();
        }

        private void ConvertData()
        {
            _vector = new BitVector32(0);
            _vector[DataSections.LIGHT] = Convert.ToInt16(_light);
            _vector[DataSections.HUMIDITY] = Convert.ToInt16(_humidity);
            _vector[DataSections.FLOODING] = Convert.ToInt16(_flooding);
            _vector[DataSections.WATER_STOCK_LEVEL] = Convert.ToInt16(_waterStock);
            _vector[DataSections.STATUS] = Convert.ToInt16(_status);

            _vector[DataSections.DATE_MINUTES] = _dateTime.Minute;
            _vector[DataSections.DATE_HOURS] = _dateTime.Hour;
            _vector[DataSections.DATE_DAY] = _dateTime.Day;
            _vector[DataSections.DATE_MONTH] = _dateTime.Month;
            _vector[DataSections.DATE_YEAR] = _dateTime.Year - 2000;

            Data = BitConverter.GetBytes(_vector.Data);
        }
    }
}