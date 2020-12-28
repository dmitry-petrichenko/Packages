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
        
        public static async Task TryCatchSuccessAsync(Task task, Action<Exception> exceptionHandler, Action successHandler)
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
        
        public static async Task<T> TryCatchWithResultAsync<T>(Func<Task<T>> action, Action<Exception> exceptionHandler, T defaultResult = default)
        {
            T result = default;
            try
            {
                var task = action();
                result = await task;
            }
            catch (Exception exception)
            {
                exceptionHandler(exception);
                return defaultResult;
            }

            return result;
        }
        
        public static T TryCatchWithResult<T>(Func<T> action, Action<Exception> exceptionHandler)
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