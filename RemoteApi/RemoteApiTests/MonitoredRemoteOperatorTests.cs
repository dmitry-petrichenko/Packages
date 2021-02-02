using System;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace RemoteApi
{
    public class MonitoredRemoteOperatorTests
    {
        private IMonitoredRemoteOperator _sut;
        private IAutoLocalConnector _autoLocalConnector;
        private IRemoteTraceMonitorСonsistent _remoteTraceMonitor;
        private IRecorder _recorder;

        public MonitoredRemoteOperatorTests()
        {
            _autoLocalConnector = Mock.Create<IAutoLocalConnector>();
            _remoteTraceMonitor = Mock.Create<IRemoteTraceMonitorСonsistent>();
            _recorder = Mock.Create<IRecorder>();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldSubscribe()
        {
            _autoLocalConnector.ArrangeSet(x => x.TextReceived += null).IgnoreArguments().Occurs(1);
            _autoLocalConnector.ArrangeSet(x => x.Connected += null).IgnoreArguments().Occurs(1);
            _remoteTraceMonitor.ArrangeSet(x => x.CommandReceived += null).IgnoreArguments().Occurs(1);
            
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            _autoLocalConnector.AssertAll();
            _remoteTraceMonitor.AssertAll();
        }
        
        [Fact]
        public void TextEntered_WhenRaised_ShouldExecuteCommand()
        {
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _remoteTraceMonitor.CommandReceived += null, "text");

            Mock.Assert(() => _autoLocalConnector.ExecuteCommand("text"), Occurs.Exactly(1));
        }
        
        [Fact]
        public void TextReceived_WhenRaised_ShouldDisplayNextMessage()
        {
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _autoLocalConnector.TextReceived += null, "2222");

            Mock.Assert(() => _remoteTraceMonitor.DisplayNextMessage("2222"), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Connected_WhenRaised_ShouldSetPrompt()
        {
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _autoLocalConnector.Connected += null, "9999aaaa");

            Mock.Assert(() => _remoteTraceMonitor.SetPrompt("9999aaaa"), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Start_WhenCalled_ShouldCallStart()
        {
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);

            _sut.Start();

            Mock.Assert(() => _autoLocalConnector.Start(), Occurs.Exactly(1));
        }
        
        #region Exceptions
        [Fact]
        public void ExecuteCommand_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _autoLocalConnector.ExecuteCommand(Arg.AnyString)).Throws<Exception>();
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _remoteTraceMonitor.CommandReceived += null, "text");

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void DisplayNextMessage_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _remoteTraceMonitor.DisplayNextMessage(Arg.AnyString)).Throws<Exception>();
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _autoLocalConnector.TextReceived += null, "2222");

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void SetPrompt_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _remoteTraceMonitor.SetPrompt(Arg.AnyString)).Throws<Exception>();
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);
            
            Mock.Raise(() => _autoLocalConnector.Connected += null, "9999aaaa");

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Start_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _autoLocalConnector.Start()).Throws<Exception>();
            _sut = new MonitoredRemoteOperator(_autoLocalConnector, _remoteTraceMonitor, _recorder);

            _sut.Start();

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        #endregion
    }
}