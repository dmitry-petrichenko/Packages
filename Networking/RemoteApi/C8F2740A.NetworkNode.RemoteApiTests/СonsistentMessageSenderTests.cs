using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    public class СonsistentMessageSenderTests
    {
        private IСonsistentMessageSender _sut;
        private IRecorder _recorder;
        private ITextToRemoteSender _textToRemoteSender;

        public СonsistentMessageSenderTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _textToRemoteSender = Mock.Create<ITextToRemoteSender>();
            _sut = new СonsistentMessageSender(_textToRemoteSender, _recorder);
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldNotThrowAndSendEvents()
        {
            var wasCalled = false;
            var wasException = false;
            
            try
            {
                _sut = new СonsistentMessageSender(_textToRemoteSender, _recorder);
            }
            catch (Exception e)
            {
                wasException = true;
            }

            Mock.Assert(() => _textToRemoteSender.TrySendText(Arg.IsAny<string>()), Occurs.Never());
            Assert.False(wasCalled);
            Assert.False(wasException);
        }

        [Fact]
        public async void SendRemote_AfterCallSeveralTimes_ShouldSendMessagesConsistently()
        {
            int trySendTextCalledTimes = 0;
            var taskCompletion = default(TaskCompletionSource<bool>);
            var testToRemoteSender = new TextToRemoteSenderMock();
            
            _sut = new СonsistentMessageSender(testToRemoteSender, _recorder);
            testToRemoteSender.TrySendTextCalled += () => trySendTextCalledTimes++;
            
            _sut.SendRemote("message1");
            _sut.SendRemote("message2");
            _sut.SendRemote("message3");
            
            await testToRemoteSender.TrySendTextCalledTask;
            await Task.Delay(500);

            Assert.Equal(1, trySendTextCalledTimes);
        }

        private class TextToRemoteSenderMock : ITextToRemoteSender
        {
            public event Action TrySendTextCalled;
            
            private TaskCompletionSource<bool> _trySendTextCalledCompletion;

            public Task TrySendTextCalledTask => _trySendTextCalledCompletion.Task;

            public TextToRemoteSenderMock()
            {
                _trySendTextCalledCompletion = new TaskCompletionSource<bool>();
            }

            public Task<bool> TrySendText(string instruction)
            {
                TrySendTextCalled?.Invoke();
                _trySendTextCalledCompletion.SetResult(true);
                
                return new TaskCompletionSource<bool>().Task;
            }
        }
    }
}