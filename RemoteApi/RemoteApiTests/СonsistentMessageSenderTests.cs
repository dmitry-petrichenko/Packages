using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using RemoteApi.Trace;
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
            var taskCompletion = default(TaskCompletionSource<bool>);
            _sut = new СonsistentMessageSender(_textToRemoteSender, _recorder);

            Mock.Arrange(() => _textToRemoteSender.TrySendText(Arg.IsAny<string>())).Returns(CreateCompletion().Task);

            _sut.SendRemote("message1");
            _sut.SendRemote("message2");
            _sut.SendRemote("message3");
            await Task.Delay(200);

            Mock.Assert(() => _textToRemoteSender.TrySendText(Arg.IsAny<string>()), Occurs.Once());
        }

        private TaskCompletionSource<bool> CreateCompletion()
        {
            var task = new TaskCompletionSource<bool>();
            return task;
        }
    }
}