using System;
using System.Collections.Generic;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    public class TraceableRemoteApiMapTests
    {
        private ITraceableRemoteApiMap _sut;
        private IRemoteApiMap _remoteApiMap;
        private IRemoteRecordsSender _remoteRecordsSender;
        private IRecorder _recorder;

        public TraceableRemoteApiMapTests()
        {
            _remoteApiMap = Mock.Create<IRemoteApiMap>();
            _remoteRecordsSender = Mock.Create<IRemoteRecordsSender>();
            _recorder = Mock.Create<IRecorder>();
        }
        
        [Fact]
        public void Constructor_WhenCalled_ShouldRegisterTraceCommand()
        {
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);
            
            Mock.Assert(() => _remoteApiMap.RegisterCommand(RemoteApiCommands.TRACE, Arg.IsAny<Action>(), Arg.AnyString), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterWrongCommand_WhenCalled_ShouldRegisterWrongCommand()
        {
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);

            Action a = () => { };
            _sut.RegisterWrongCommandHandler(a);
            
            Mock.Assert(() => _remoteApiMap.RegisterWrongCommandHandler(a), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterCommand_WhenCalled_ShouldRegisterCommand()
        {
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);

            Action a = () => { };
            _sut.RegisterCommand("action", a, string.Empty);
            
            Mock.Assert(() => _remoteApiMap.RegisterCommand("action", a, string.Empty), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterCommandWithParameters_WhenCalled_ShouldRegisterCommandWithParameters()
        {
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);

            Action<IEnumerable<string>> a = arr => { };
            _sut.RegisterCommandWithParameters("action", a, string.Empty);
            
            Mock.Assert(() => _remoteApiMap.RegisterCommandWithParameters("action", a, string.Empty), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void TraceCommand_WhenCalled_ShouldActivateAndSendCache()
        {
            var remoteApiMap = new RemoteApiMapMock();
            _sut = new TraceableRemoteApiMap(remoteApiMap, _remoteRecordsSender, _recorder);

            remoteApiMap.TriggerTraceCommand();
            
            Mock.Assert(() => _remoteRecordsSender.ActivateAndSendCache(), 
                Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterWrongCommandHandler_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _remoteApiMap.RegisterWrongCommandHandler(Arg.IsAny<Action>())).Throws<Exception>();
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);

            Action a = () => { };
            _sut.RegisterWrongCommandHandler(a);

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterCommand_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _remoteApiMap.RegisterCommand("action", Arg.IsAny<Action>(), string.Empty)).Throws<Exception>();
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);
            Action a = () => { };
            
            _sut.RegisterCommand("action", a, string.Empty);

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void RegisterCommandWithParameters_WhenThrows_ShouldCatch()
        {
            Mock.Arrange(() => _remoteApiMap.RegisterCommandWithParameters("action", Arg.IsAny<Action<IEnumerable<string>>>(), string.Empty)).Throws<Exception>();
            _sut = new TraceableRemoteApiMap(_remoteApiMap, _remoteRecordsSender, _recorder);
            Action<IEnumerable<string>> a = arr => { };
            
            _sut.RegisterCommandWithParameters("action", a, string.Empty);

            Mock.Assert(() => _recorder.DefaultException(Arg.IsAny<Object>(), Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        private class RemoteApiMapMock : IRemoteApiMap
        {
            private Action _handler;
            
            public void RegisterWrongCommandHandler(Action action)
            {
                throw new NotImplementedException();
            }

            public void TriggerTraceCommand()
            {
                _handler?.Invoke();
            }

            public void RegisterCommand(string name, Action handler, string description = "")
            {
                if (name.Equals(RemoteApiCommands.TRACE))
                {
                    _handler = handler;
                }
            }

            public void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "")
            {
                throw new NotImplementedException();
            }
        }
    }
}