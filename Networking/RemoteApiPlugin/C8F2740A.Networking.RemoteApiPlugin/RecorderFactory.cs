using System;
using C8F2740A.NetworkNode.RemoteApi;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public class RecorderFactory
    {
        private readonly IConfiguration _configuration;
        private readonly Action<string> _interruptionHandler;

        private ISystemRecorder _systemRecorder;
        private IApplicationRecorder _applicationRecorder;
        
        public RecorderFactory(IConfiguration configuration, Action<string> interruptionHandler)
        {
            _configuration = configuration;
            _interruptionHandler = interruptionHandler;
        }
        
        public ISystemRecorder CreateSystemRecorder()
        {
            if (_systemRecorder == default)
            {
                _systemRecorder = new SystemRecorder();
                _systemRecorder.InterruptedWithMessage += InterruptionHandler;

                return _systemRecorder;
            }
            
            return _systemRecorder;
        }
        
        public IApplicationRecorder CreateApplicationRecorder()
        {
            if (_applicationRecorder == null)
            {
                var systemRecorder = CreateSystemRecorder();
            
                // Application recorder
                _applicationRecorder = new ApplicationRecorder(
                    systemRecorder, 
                    new MessagesCache(Int32.Parse(_configuration["MESSAGE_CACHE"])));
                ((IApplicationRecorder)_applicationRecorder).RecordInfo("system", "started");

                return _applicationRecorder;
            }
            
            return _applicationRecorder;
        }

        private void InterruptionHandler(string message)
        {
            _systemRecorder.InterruptedWithMessage -= InterruptionHandler;
            _interruptionHandler?.Invoke(message);
        }
    }
}