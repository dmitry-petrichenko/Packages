namespace RemoteApi.Trace
{
    public interface IInternalExceptionHandler
    {
        void LogException(string message);
    }
    
    public class InternalExceptionHandler : IInternalExceptionHandler
    {
        public void LogException(string message)
        {
            var m = message;
        }
    }
}