using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.Storage.QueuesStorage;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public interface IServiceBuilder
    {
        Task<IServiceRunner> Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IStorage, Task<IRunnable>> setupCore, string settingsPath);
    }
}