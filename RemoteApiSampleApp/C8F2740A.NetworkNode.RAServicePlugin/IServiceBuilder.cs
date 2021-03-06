﻿using System;
using C8F2740A.NetworkNode.RemoteApi.Trace;

namespace C8F2740A.NetworkNode.RAServicePlugin
{
    public interface IServiceBuilder
    {
        IServiceRunner Build(Func<ITraceableRemoteApiMap, IApplicationRecorder, IRunnable> setupCore);
    }
}