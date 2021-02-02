using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Monitor;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using RemoteApi;

namespace RemoteOperatorWithFactories
{
    class Program
    {
        private static IApplicationRecorder _recorder;
        private static IRemoteTraceMonitor _remoteTraceMonitor;
        private static IMonitoredRemoteOperator _monitoredRemoteOperator;

        private static TaskCompletionSource<bool> _mainApplicationTask;
        
        static async Task Main(string[] args)
        {
            await CreateApplicationBuilder().Build().Run();
        }

        public static IApplicationBuildable CreateApplicationBuilder()
        {
            return new ApplicationBuilder();
        }
    }
}