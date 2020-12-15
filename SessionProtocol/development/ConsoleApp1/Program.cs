using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static byte REQUEST = 0b1100_0000;
        private static byte RESPONSE = 0b0011_0000;
        
        static void Main(string[] args)
        {

            var t = new Program().GetType();
            var res1 = IsAsyncMethod(t, "MethodTask");
            var res2 = IsAsyncMethod(t, "MethodTaskAsync");

            Console.WriteLine($"{res1} {res2}");
            //var res = arr.Skip(1).ToArray();

            //Console.Read();
        }

        public Task MethodTask()
        {
            return Task.CompletedTask;
        }
        
        public async Task MethodTaskAsync()
        {
            await Task.Delay(100);
            return;
        }
        
        private static bool IsAsyncMethod(Type classType, string methodName)
        {
            // Obtain the method with the specified name.
            MethodInfo method = classType.GetMethod(methodName);

            Type attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method.
            // The value returned contains the StateMachineType property.
            // Null is returned if the attribute isn't present for the method.
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }
        
        public static async Task<T> TryCatchWithResultAsync<T>(Func<Task<T>> action, Action<Exception> exceptionHandler)
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
                return default;
            }

            return result;
        }
    }
}