using System;
using System.Collections.Generic;

namespace RemoteApi.Trace
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
        
        public TraceableRemoteApiMap(
            IRemoteApiMap remoteApiMap,
            IRemoteRecordsSender remoteRecordsSender)
        {
            _remoteApiMap = remoteApiMap;
            _remoteRecordsSender = remoteRecordsSender;
            
            _remoteApiMap.RegisterCommand(RemoteApiCommands.TRACE, TraceHandler);
        }

        private void TraceHandler()
        {
            _remoteRecordsSender.ActivateAndSendCache();
            
            // TODO Trace Started
        }

        public void RegisterWrongCommandHandler(Action action)
        {
            _remoteApiMap.RegisterWrongCommandHandler(action);
        }

        public void RegisterCommand(string name, Action handler, string description = "")
        {
            _remoteApiMap.RegisterCommand(name, handler, description);
        }

        public void RegisterCommandWithParameters(string name, Action<IEnumerable<string>> handler, string description = "")
        {
            _remoteApiMap.RegisterCommandWithParameters(name, handler, description);
        }
    }
}