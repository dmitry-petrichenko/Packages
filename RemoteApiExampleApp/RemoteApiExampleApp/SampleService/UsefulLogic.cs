using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RAServicePlugin;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace SampleService
{
    public class UsefulLogic : IRunnable
    {
        private readonly IApplicationRecorder _recorder;
        private int _currentValue;
            
        public UsefulLogic(IApplicationRecorder recorder)
        {
            _recorder = recorder;
            _currentValue = 0;
        }
            
        public void SetValue(int value)
        {
            _currentValue = value;
        }

        private async Task StartProcess()
        {
            while (_currentValue < Int32.MaxValue)
            {
                await Task.Delay(800);
                _currentValue++;
                _recorder.RecordInfo("App", $"value: {_currentValue}");
            }
        }

        public void Run()
        {
            StartProcess();
        }
    }
}