using System.ServiceModel;
using System.Threading.Tasks;
using Service.Core.Client.Models;
using Service.Registration.Grpc.Models;

namespace Service.Registration.Grpc
{
	[ServiceContract]
	public interface IRegistrationService
	{
		[OperationContract]
		ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request);

		[OperationContract]
		ValueTask<ConfirmRegistrationGrpcResponse> ConfirmRegistrationAsync(ConfirmRegistrationGrpcRequest request);
	}
}