using System;

namespace RemoteApi.Integration.Helpers.SocketsSubstitution
{
    public class ExceptionLine
    {
        public ExceptionLine()
        {
            Value = () => { };
        }

        public Action Value { get; set; }
    }
}