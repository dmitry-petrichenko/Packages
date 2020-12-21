using System;
using Autofac;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.SessionTCP;
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

            _containerBuilder.RegisterType<Test>();
            //--------------
            
            return this;
        }

        public void Run()
        {
            _containerBuilder.Build().Resolve<Test>();
        }
    }
}