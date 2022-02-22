using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Core.Client.Services;
using Service.EducationProgress.Client;
using Service.Registration.Models;
using Service.Registration.Services;
using Service.ServiceBus.Models;
using Service.UserAccount.Client;
using Service.UserInfo.Crud.Client;

namespace Service.Registration.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RegistrationService>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<HashCodeService<EmailHashDto>>().As<IHashCodeService<EmailHashDto>>().SingleInstance();

			builder.RegisterUserInfoCrudClient(Program.Settings.UserInfoCrudServiceUrl, Program.LogFactory.CreateLogger(typeof(UserInfoCrudClientFactory)));
			builder.RegisterUserAccountClient(Program.Settings.UserAccountServiceUrl, Program.LogFactory.CreateLogger(typeof(UserAccountClientFactory)));

			builder.RegisterEducationProgressClient(Program.Settings.EducationProgressServiceUrl);

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.Registration");

			builder
				.Register(context => new MyServiceBusPublisher<RegistrationInfoServiceBusModel>(tcpServiceBus, RegistrationInfoServiceBusModel.TopicName, false))
				.As<IServiceBusPublisher<RegistrationInfoServiceBusModel>>()
				.SingleInstance();

			tcpServiceBus.Start();
		}
	}
}