using System.Runtime.Serialization;

namespace Service.Registration.Grpc.Models
{
	[DataContract]
	public class ConfirmRegistrationGrpcResponse
	{
		[DataMember(Order = 1)]
		public string Email { get; set; }
	}
}