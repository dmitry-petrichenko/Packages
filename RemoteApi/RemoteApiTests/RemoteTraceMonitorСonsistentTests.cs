using System.Dynamic;
using System.Threading.Tasks;
using RemoteApi.Monitor;
using Telerik.JustMock;
using Xunit;

namespace RemoteApi
{
    public class RemoteTraceMonitorСonsistentTests
    {
        private IRemoteTraceMonitorСonsistent _sut;
        private IRemoteTraceMonitor _remoteTraceMonitor;

        public RemoteTraceMonitorСonsistentTests()
        {
            _remoteTraceMonitor = Mock.Create<IRemoteTraceMonitor>();
        }
        
        [Fact]
        public void CommandReceived_WhenTextEnteredCalled_ShouldRaiseСonsistent()
        {
            dynamic resultCache = new ExpandoObject();
            var beforeTask = new TaskCompletionSource<bool>();
            var afterTask = new TaskCompletionSource<bool>();
            var commandReceivedTimes = 0;
            var lastCommandReceivedValue = string.Empty;
            async Task<bool> CommandReceivedHandler(string command)
            {
                beforeTask.SetResult(true);
                lastCommandReceivedValue = command;
                commandReceivedTimes++;
                await Task.Delay(300);
                afterTask.SetResult(true);
                
                return true;
            }
            _sut = new RemoteTraceMonitorСonsistent(_remoteTraceMonitor);
            _sut.CommandReceived += CommandReceivedHandler;

            Mock.Raise(() => _remoteTraceMonitor.TextEntered += null, "capacity1");
            Mock.Raise(() => _remoteTraceMonitor.TextEntered += null, "capacity2");

            beforeTask.Task.Wait();
            resultCache.commandReceived1 = lastCommandReceivedValue;
            resultCache.commandReceivedTimes1 = commandReceivedTimes;
            afterTask.Task.Wait();
            
            beforeTask = new TaskCompletionSource<bool>();
            afterTask = new TaskCompletionSource<bool>();
            
            beforeTask.Task.Wait();
            resultCache.commandReceived2 = lastCommandReceivedValue;
            resultCache.commandReceivedTimes2 = commandReceivedTimes;
            afterTask.Task.Wait();
            
            Assert.Equal(1, resultCache.commandReceivedTimes1);
            Assert.Equal("capacity1", resultCache.commandReceived1);
            Assert.Equal(2, resultCache.commandReceivedTimes2);
            Assert.Equal("capacity2", resultCache.commandReceived2);
        }

    }
}