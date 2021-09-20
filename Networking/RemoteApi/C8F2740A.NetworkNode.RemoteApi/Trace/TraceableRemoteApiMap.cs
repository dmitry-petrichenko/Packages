using System;
using System.Collections.Generic;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.RemoteApi.Trace
{
    public interface ITraceableRemoteApiMap
    {
        void RegisterWrongCommandHandler(Action action);
        void RegisterCommand(string name, Action handler, string description = "");
        void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "");
    }
    
    public class TraceableRemoteApiMap : ITraceableRemoteApiMap
    {
        private readonly IRemoteApiMap _remoteApiMap;
        private readonly IRemoteRecordsSender _remoteRecordsSender;
        private readonly IRecorder _recorder;
        private readonly IApplicationRecorder _applicationRecorder;

        internal Action TraceStarted;
        
        public TraceableRemoteApiMap(
            IRemoteApiMap remoteApiMap,
            IRemoteRecordsSender remoteRecordsSender,
            IApplicationRecorder applicationRecorder,
            IRecorder recorder)
        {
            _remoteApiMap = remoteApiMap;
            _remoteRecordsSender = remoteRecordsSender;
            _applicationRecorder = applicationRecorder;
            _recorder = recorder;
            
            RegisterCommand(RemoteApiCommands.TRACE, TraceHandler);
            RegisterCommand("commands", CommandsHandler);
        }

        private void TraceHandler()
        {
            _remoteRecordsSender.ActivateAndSendCache();
            
            TraceStarted?.Invoke();
        }
        
        private void CommandsHandler()
        {
            var commands = GetCommands();
            var commandsString = string.Join(" ", commands);
            _applicationRecorder.RecordInfo("", commandsString);
        }

        private IEnumerable<string> GetCommands()
        {
            return _remoteApiMap.GetCommands();
        }

        public void RegisterWrongCommandHandler(Action action)
        {
            SafeExecution.TryCatch(() => _remoteApiMap.RegisterWrongCommandHandler(action),
                exception => _recorder.DefaultException(this, exception));
        }

        public void RegisterCommand(string name, Action handler, string description = "")
        {
            SafeExecution.TryCatch(() => _remoteApiMap.RegisterCommand(name, handler, description),
                exception => _recorder.DefaultException(this, exception));
        }

        public void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "")
        {
            SafeExecution.TryCatch(() => _remoteApiMap.RegisterCommandWithParameters(name, handler, description),
                exception => _recorder.DefaultException(this, exception));
        }
    }
}