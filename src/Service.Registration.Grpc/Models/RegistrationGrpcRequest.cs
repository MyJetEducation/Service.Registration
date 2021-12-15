using System.Runtime.Serialization;

namespace Service.Registration.Grpc.Models
{
	[DataContract]
	public class RegistrationGrpcRequest
	{
		[DataMember(Order = 1)]
		public string UserName { get; set; }

		[DataMember(Order = 1)]
		public string Password { get; set; }
	}
}