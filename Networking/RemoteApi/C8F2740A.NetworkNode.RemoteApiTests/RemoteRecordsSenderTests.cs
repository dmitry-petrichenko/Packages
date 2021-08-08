using System.Linq;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace RemoteApi
{
    public class RemoteRecordsSenderTests
    {
        private IRemoteRecordsSender _sut;
        private IСonsistentMessageSender _consistentMessageSender;
        private IApplicationRecorder _applicationRecorder;
        private IRecorder _recorder;

        public RemoteRecordsSenderTests()
        {
            _consistentMessageSender = Mock.Create<IСonsistentMessageSender>();
            _applicationRecorder = Mock.Create<IApplicationRecorder>();
            _recorder = Mock.Create<IRecorder>();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            _applicationRecorder.ArrangeSet(x => x.RecordReceived += null).IgnoreArguments().Occurs(1);
            
            _sut = new RemoteRecordsSender(_consistentMessageSender, _applicationRecorder, _recorder);
            
            _applicationRecorder.AssertAll();
        }
        
        [Fact]
        public void ActivateAndSendCache_WhenCalled_ShouldSendCorrect()
        {
            Mock.Arrange(() => _applicationRecorder.GetCache()).Returns(Enumerable.Empty<string>());
            _sut = new RemoteRecordsSender(_consistentMessageSender, _applicationRecorder, _recorder);
            
            _sut.ActivateAndSendCache();
            
            Mock.Assert(() => _consistentMessageSender.SendRemote(string.Empty), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void ActivateAndSendCache_WhenCalled_ShouldSendCorrect2()
        {
            Mock.Arrange(() => _applicationRecorder.GetCache()).Returns(new [] { "message1", "message2" });
            _sut = new RemoteRecordsSender(_consistentMessageSender, _applicationRecorder, _recorder);
            
            _sut.ActivateAndSendCache();
            
            Mock.Assert(() => _consistentMessageSender.SendRemote("message1\r\nmessage2\r\n"), 
                Occurs.Exactly(1));
        }
    }
}