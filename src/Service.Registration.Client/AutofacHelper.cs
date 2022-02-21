using Autofac;
using Microsoft.Extensions.Logging;
using Service.Grpc;
using Service.Registration.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Registration.Client
{
	public static class AutofacHelper
	{
		public static void RegisterRegistrationClient(this ContainerBuilder builder, string grpcServiceUrl, ILogger logger)
		{
			var factory = new RegistrationClientFactory(grpcServiceUrl, logger);

			builder.RegisterInstance(factory.GetRegistrationService()).As<IGrpcServiceProxy<IRegistrationService>>().SingleInstance();
		}
	}
}