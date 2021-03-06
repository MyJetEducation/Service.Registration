using System.Runtime.Serialization;

namespace Service.Registration.Grpc.Models
{
	[DataContract]
	public class RegistrationGrpcRequest
	{
		[DataMember(Order = 1)]
		public string UserName { get; set; }

		[DataMember(Order = 2)]
		public string Password { get; set; }

		[DataMember(Order = 3)]
		public string FirstName { get; set; }

		[DataMember(Order = 4)]
		public string LastName { get; set; }
	}
}