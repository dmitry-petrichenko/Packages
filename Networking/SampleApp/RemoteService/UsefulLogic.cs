﻿using System.Threading.Tasks;
using C8F2740A.Networking.RemoteApiPlugin;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace SampleService
{
    public class UsefulLogic : IUpable
    {
        private readonly IApplicationRecorder _recorder;
        private ulong _currentValue;
            
        public UsefulLogic(IApplicationRecorder recorder)
        {
            _recorder = recorder;
            _currentValue = 0;
        }
            
        public void SetValue(int value)
        {
            _currentValue = (uint)value;
        }

        private async Task StartProcess()
        {
            while (_currentValue < 1000)
            {
                await Task.Delay(1000);
                _currentValue++;
                _recorder.RecordInfo("app", $"value: {_currentValue}");
            }
        }

        public void Up()
        {
            StartProcess();
        }
    }
}