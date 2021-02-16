﻿using System;
using System.Linq;
using System.Threading.Tasks;
using C8F2740A.NetworkNode.RemoteApi.Trace;
using C8F2740A.NetworkNode.RemoteApiServicePlugin;

namespace SampleService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateApplicationBuilder().Build(SetupCoreHandler).Run();
            
            Console.ReadLine();
        }

        private static IRunnable SetupCoreHandler(ITraceableRemoteApiMap map, IApplicationRecorder recorder)
        {
            recorder.RecordReceived += s => Console.WriteLine(s);
            
            // Create core logic here
            var core = new UsefulLogic(recorder);
            
            // Register commands here
            map.RegisterCommandWithParameters("set", parameter =>
            {
                core.SetValue(Int32.Parse(parameter.FirstOrDefault()));
            });
            
            return core;
        }

        public static IServiceBuilder CreateApplicationBuilder()
        {
            return new ServiceSkeleton();
        }
    }
}