using System.Runtime.Serialization;

namespace Service.Registration.Domain.Models
{
	[DataContract]
	public class RegistrationInfoServiceBusModel : IRegistrationInfo
	{
		public const string TopicName = "myjeteducation-registration-confirm";

		[DataMember(Order = 1)]
		public string Email { get; set; }

		[DataMember(Order = 2)]
		public string Hash { get; set; }
	}
}