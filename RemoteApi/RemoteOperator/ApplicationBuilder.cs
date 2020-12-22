using System;
using Autofac;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.RemoteApi.Nuget.Trace;
using C8F2740A.NetworkNode.SessionTCP;
using C8F2740A.NetworkNode.SessionTCP.Factories;
using RemoteApi;
using RemoteApi.Trace;

namespace RemoteOperator
{
    public class ApplicationBuilder
    {
        private ContainerBuilder _containerBuilder;
        
        public ApplicationBuilder()
        {
            _containerBuilder = new ContainerBuilder();
        }
        
        public ApplicationBuilder Build()
        {
            // SessionTCP layer
            var recorder = new RecorderStream();
            _containerBuilder.Register(c => recorder).As<IRecorderStream>().SingleInstance();
            _containerBuilder.Register(c => recorder).As<IRecorder>().SingleInstance();
            
            _containerBuilder.RegisterType<NetworkConnector>().As<INetworkConnector>().SingleInstance();
            _containerBuilder.RegisterType<SocketFactory>().As<ISocketFactory>().SingleInstance();

            _containerBuilder.Register<Func<ISocket, INetworkTunnel>>(
                c =>
                    socket => new NetworkTunnel(socket, c.Resolve<IRecorder>()));

            _containerBuilder.Register(c => new NetworkAddress("127.0.0.1:25757")).As<INetworkAddress>();
            
            _containerBuilder.RegisterType<NodeGateway>().As<INodeGateway>().SingleInstance();
            _containerBuilder.RegisterType<NodeVisitor>().As<INodeVisitor>().SingleInstance();
            
            _containerBuilder.Register<Func<INetworkTunnel, ISession>>(
                c =>
                    tunnel => new Session(tunnel, c.Resolve<IRecorder>()));

            _containerBuilder.RegisterType<NetworkPoint>().As<INetworkPoint>();
            _containerBuilder.RegisterType<DefaultInstructionSenderFactory>().As<IInstructionSenderFactory>();
            _containerBuilder.RegisterType<InstructionSenderHolder>().As<IInstructionSenderHolder>();
            _containerBuilder.RegisterType<RemoteApiOperator>().As<IRemoteApiOperator>();
            _containerBuilder.RegisterType<SessionHolder>().As<ISessionHolder>();
            
            _containerBuilder.RegisterType<LocalConsolePoint>();
            _containerBuilder.RegisterType<ExternalConsolePoint>().As<IExternalConsolePoint>();
            _containerBuilder.RegisterType<MessageStreamer>().As<IMessageStreamer>();
            _containerBuilder.RegisterType<RemoteApiMap>().As<IRemoteApiMap>();
            _containerBuilder.RegisterType<InstructionReceiver>().As<IInstructionReceiver>();
            
            _containerBuilder.RegisterType<TraceMonitorFacade>().As<ITraceMonitorFacade>();
            _containerBuilder.Register(c => new RemoteTraceMonitor(8)).As<IRemoteTraceMonitor>();
            _containerBuilder.RegisterType<ConsoleOperatorBootstrapper>().As<IConsoleOperatorBootstrapper>();
            //--------------
            
            return this;
        }

        public void Run()
        {
            var context = _containerBuilder.Build();
            var streamer = context.Resolve<IMessageStreamer>();
            streamer.SetLocalStreaming(true);
            var localPoint = context.Resolve<LocalConsolePoint>();
            var facade = context.Resolve<ITraceMonitorFacade>();
            facade.Start();
        }
    }
}