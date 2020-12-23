using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    public class СonsistentMessageSenderTests
    {
        private IСonsistentMessageSender _sut;
        private IInternalExceptionHandler _internalExceptionHandler;

        public СonsistentMessageSenderTests()
        {
            _internalExceptionHandler = Mock.Create<IInternalExceptionHandler>();
            _sut = new СonsistentMessageSender(_internalExceptionHandler);
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldNotThrowAndSendEvents()
        {
            var wasCalled = false;
            var wasException = false;
            _sut.SendMessage += bytes =>
            {
                wasCalled = true;
                return default;
            };
            
            try
            {
                _sut = new СonsistentMessageSender(_internalExceptionHandler);
            }
            catch (Exception e)
            {
                wasException = true;
            }

            Assert.False(wasCalled);
            Assert.False(wasException);
        }

        [Fact]
        public async void SendRemote_AfterCallSeveralTimes_ShouldSendMessagesConsistently()
        {
            var taskCompletion = default(TaskCompletionSource<bool>);
            var calledTimes = 0;
            _sut = new СonsistentMessageSender(_internalExceptionHandler);
            _sut.SendMessage += async data =>
            {
                calledTimes++;
                taskCompletion = CreateCompletion();
                await taskCompletion.Task;
                return (true, Enumerable.Empty<byte>());
            };
            _sut.SendRemote("message1");
            _sut.SendRemote("message2");
            _sut.SendRemote("message3");
            await Task.Delay(200);

            Assert.Equal(calledTimes, 1);
        }

        private TaskCompletionSource<bool> CreateCompletion()
        {
            var task = new TaskCompletionSource<bool>();
            return task;
        }
    }
}