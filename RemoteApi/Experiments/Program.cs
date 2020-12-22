using System;
using Autofac;
using Autofac.Core;

namespace Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<Class11>().As<IClass11>();
            containerBuilder.Register(c => new Sender(17))
                .As<ISender>()
                .Keyed<ISender>("17");
            
            containerBuilder.Register(c => new Sender(6))
                .As<ISender>()
                .Keyed<ISender>("6");
            
            containerBuilder.RegisterType<Class2>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(ISender),
                        (pi, ctx) => ctx.ResolveKeyed<ISender>("17")))
                .As<IClass2>();
            
            containerBuilder.RegisterType<Class1>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType == typeof(ISender),
                        (pi, ctx) => ctx.ResolveKeyed<ISender>("6")))
                .As<IClass1>();

            var c = containerBuilder.Build();
            //var v1 = c.Resolve<IClass1>();
            var v2 = c.Resolve<IClass2>();

            Console.ReadKey();
        }
    }
    
    public interface IClass1
    {
        
    }

    public class Class1 : IClass1
    {
        public Class1(IClass11 class11)
        {
        }
    }
    
    public interface IClass2
    {
        
    }
    
    public class Class2 : IClass2
    {
        public Class2(IClass11 class11)
        {
        }
    }

    public interface IClass11
    {
        
    }
    
    public class Class11 : IClass11
    {
        public Class11(ISender senderWith)
        {
            Console.WriteLine(senderWith.Index);
        }
    }

    public interface ISender
    {
        int Index { get; }
    }
    
    public class Sender : ISender
    {
        public Sender(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}