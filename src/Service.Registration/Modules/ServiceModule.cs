using Autofac;
using DotNetCoreDecorators;
using MyServiceBus.TcpClient;
using Service.Core.Client.Services;
using Service.EducationProgress.Client;
using Service.Registration.Models;
using Service.Registration.Services;
using Service.ServiceBus.Models;
using Service.ServiceBus.Services;
using Service.UserInfo.Crud.Client;
using Service.UserProfile.Client;

namespace Service.Registration.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RegistrationService>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<HashCodeService<EmailHashDto>>().As<IHashCodeService<EmailHashDto>>().SingleInstance();

			builder.RegisterUserInfoCrudClient(Program.Settings.UserInfoCrudServiceUrl);
			builder.RegisterEducationProgressClient(Program.Settings.EducationProgressServiceUrl);
			builder.RegisterUserProfileClient(Program.Settings.UserProfileServiceUrl);

			MyServiceBusTcpClient tcpServiceBus = builder.RegisterServiceBusClient(Program.ReloadedSettings(e => e.ServiceBusWriter), Program.LogFactory);

			builder
				.RegisterInstance(new ServiceBusPublisher<RegistrationInfoServiceBusModel>(tcpServiceBus, RegistrationInfoServiceBusModel.TopicName, false))
				.As<IPublisher<RegistrationInfoServiceBusModel>>()
				.SingleInstance();

			tcpServiceBus.Start();
		}
	}
}