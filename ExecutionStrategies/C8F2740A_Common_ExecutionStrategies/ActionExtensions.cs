using System;

namespace C8F2740A.Common.ExecutionStrategies
{
    public static class ActionExtensions
    {
        public static void TryCatch(this Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
            }
        }
        
        public static T TryCatch<T>(this Func<T> action, Action<Exception> exceptionHandler)
        {
            T result = default;
            try
            {
                result = action();
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
            }

            return result;
        }
    }
}