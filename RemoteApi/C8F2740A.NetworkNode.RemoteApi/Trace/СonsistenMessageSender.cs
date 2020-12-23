using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;

namespace RemoteApi.Trace
{
    public interface IСonsistentMessageSender
    {
        void SendRemote(string message);
        
        event Func<IEnumerable<byte>, Task<(bool, IEnumerable<byte>)>> SendMessage;
    }
    
    public class СonsistentMessageSender : IСonsistentMessageSender
    {
        private readonly IInternalExceptionHandler _internalExceptionHandler;
        
        private BlockingCollection<string> _messageQueue;
        private Task _sendingTask;

        public СonsistentMessageSender(IInternalExceptionHandler internalExceptionHandler)
        {
            _messageQueue = new BlockingCollection<string>();
            _internalExceptionHandler = internalExceptionHandler;

            StartProducing();
        }

        public void SendRemote(string message)
        {
            _messageQueue.Add(message);
        }
        
        private void StartProducing()
        {
            SafeExecution.TryCatchAsync(StartProducingInternal(),
                exception => _internalExceptionHandler.LogException($"{GetType().Name}: {exception.Message}"));
        }

        private async Task StartProducingInternal()
        {
            await Task.Run(async () =>
            {
                try
                {
                    foreach (var element in _messageQueue.GetConsumingEnumerable())
                    {
                        await ExecuteSendMessage(element);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            });
            int i = 0;
        }

        private async Task ExecuteSendMessage(string message)
        {
            var result = await SendMessage.Invoke(message.ToEnumerableByte());

            if (!result.Item1)
            {
                _internalExceptionHandler.LogException($"{GetType().Name}: Message did not sent");
            }
        }

        public event Func<IEnumerable<byte>, Task<(bool, IEnumerable<byte>)>> SendMessage;
    }
}