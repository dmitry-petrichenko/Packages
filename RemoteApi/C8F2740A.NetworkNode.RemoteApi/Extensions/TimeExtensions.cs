using System;

namespace C8F2740A.NetworkNode.RemoteApi.Extensions
{
    public static class TimeExtensions
    {
        public static string ToShortTimeFormat(this DateTime dateTime)
        {
            var seconds = dateTime.Second >= 10 ? dateTime.Second.ToString() : $"0{dateTime.Second}";
            var minutes = dateTime.Minute >= 10 ? dateTime.Minute.ToString() : $"0{dateTime.Minute}";
            var hours = dateTime.Hour >= 10 ? dateTime.Hour.ToString() : $"0{dateTime.Hour}";
            return $"{hours}:{minutes}:{seconds}";
        }
    }
}