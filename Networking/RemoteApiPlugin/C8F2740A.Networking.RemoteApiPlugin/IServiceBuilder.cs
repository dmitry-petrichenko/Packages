using System;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Networking.RemoteApiPlugin
{
    public interface IServiceBuilder
    {
        IServiceRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IConfiguration, IUpable> setupCore, string settingsPath);
    }
}