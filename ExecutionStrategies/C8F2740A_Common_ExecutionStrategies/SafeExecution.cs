using System;
using System.Threading.Tasks;

namespace C8F2740A.Common.ExecutionStrategies
{
    public static class SafeExecution
    {
        public static void TryCatch(Action action, Action<Exception> exceptionHandler)
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
        
        public static async Task TryCatchAsync(Task task, Action<Exception> exceptionHandler)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
            }
        }
        
        public static async Task TryCatchSuccessAsync(Task task, Action successHandler, Action<Exception> exceptionHandler)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
                return;
            }

            successHandler?.Invoke();
        }
    }
}