using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Registration.Domain.Models;

namespace Service.Registration.Services
{
	public class MyServiceBusPublisher : IPublisher<IRegistrationInfo>
	{
		private readonly MyServiceBusTcpClient _client;

		public MyServiceBusPublisher(MyServiceBusTcpClient client)
		{
			_client = client;
			_client.CreateTopicIfNotExists(RegistrationInfoServiceBusModel.TopicName);
		}

		public ValueTask PublishAsync(IRegistrationInfo valueToPublish)
		{
			var serviceBusModel = new RegistrationInfoServiceBusModel
			{
				Email = valueToPublish.Email,
				Hash = valueToPublish.Hash
			};

			byte[] bytesToSend = serviceBusModel.ServiceBusContractToByteArray();

			Task task = _client.PublishAsync(RegistrationInfoServiceBusModel.TopicName, bytesToSend, false);

			return new ValueTask(task);
		}
	}
}