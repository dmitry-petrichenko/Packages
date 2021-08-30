using System;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.Storages.QueuesStorage;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public interface IServiceBuilder
    {
        IServiceRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IStorage, IRunnable> setupCore, string settingsPath);
    }
}