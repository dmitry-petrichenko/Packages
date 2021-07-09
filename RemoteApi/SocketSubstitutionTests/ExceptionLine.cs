using System;

namespace SocketSubstitutionTests
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