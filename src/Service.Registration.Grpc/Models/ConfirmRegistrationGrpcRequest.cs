using System.Runtime.Serialization;

namespace Service.Registration.Grpc.Models
{
	[DataContract]
	public class ConfirmRegistrationGrpcRequest
	{
		[DataMember(Order = 1)]
		public string Hash { get; set; }
	}
}