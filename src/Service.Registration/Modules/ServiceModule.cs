﻿using Autofac;
using DotNetCoreDecorators;
using MyServiceBus.TcpClient;
using Service.Core.Domain;
using Service.Core.Domain.Models;
using Service.EducationProgress.Client;
using Service.Registration.Domain.Models;
using Service.Registration.Models;
using Service.Registration.Services;
using Service.UserInfo.Crud.Client;
using Service.UserProfile.Client;

namespace Service.Registration.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterUserInfoCrudClient(Program.Settings.UserInfoCrudServiceUrl);
			builder.RegisterType<RegistrationService>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<HashCodeService<EmailHashDto>>().As<IHashCodeService<EmailHashDto>>().SingleInstance();
			builder.RegisterEducationProgressClient(Program.Settings.EducationProgressServiceUrl);
			builder.RegisterUserProfileClient(Program.Settings.UserProfileServiceUrl);

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.Registration");
			IPublisher<IRegistrationInfo> clientRegisterPublisher = new MyServiceBusPublisher(tcpServiceBus);
			builder.Register(context => clientRegisterPublisher);
			tcpServiceBus.Start();
		}
	}
}