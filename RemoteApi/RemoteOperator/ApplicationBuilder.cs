using Autofac;

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
            /*
            _containerBuilder.Register(c => recorder).As<IRecorder>().SingleInstance();
            
            _containerBuilder.RegisterType<NetworkConnector>().As<INetworkConnector>().SingleInstance();
            _containerBuilder.RegisterType<SocketFactory>().As<ISocketFactory>().SingleInstance();
            */
            
            return this;
        }

        public void Run()
        {
            /*
            var facade = context.Resolve<ITraceMonitorFacade>();
            facade.Start();
            */
        }
    }
}