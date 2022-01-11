using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Registration.Domain.Models;

namespace Service.Registration.Services
{
	public class MyServiceBusPublisher : IPublisher<RegistrationInfoServiceBusModel>
	{
		private readonly MyServiceBusTcpClient _client;

		public MyServiceBusPublisher(MyServiceBusTcpClient client)
		{
			_client = client;
			_client.CreateTopicIfNotExists(RegistrationInfoServiceBusModel.TopicName);
		}

		public ValueTask PublishAsync(RegistrationInfoServiceBusModel valueToPublish)
		{
			byte[] bytesToSend = valueToPublish.ServiceBusContractToByteArray();

			Task task = _client.PublishAsync(RegistrationInfoServiceBusModel.TopicName, bytesToSend, false);

			return new ValueTask(task);
		}
	}
}