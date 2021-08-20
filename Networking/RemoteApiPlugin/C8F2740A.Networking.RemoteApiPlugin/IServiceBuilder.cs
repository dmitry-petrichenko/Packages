using System;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.Storage.QueuesStorage;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public interface IServiceBuilder
    {
        IServiceRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IStorage, IRunnable> setupCore, string settingsPath);
    }
}