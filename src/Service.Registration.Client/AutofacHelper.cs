using Autofac;
using Service.Registration.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Registration.Client
{
    public static class AutofacHelper
    {
        public static void RegisterRegistrationClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new RegistrationClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
