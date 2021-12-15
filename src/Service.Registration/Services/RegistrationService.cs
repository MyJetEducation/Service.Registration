using System.Text.Json;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Core.Domain.Models;
using Service.Core.Grpc.Models;
using Service.Registration.Domain.Models;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.Registration.Models;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;

namespace Service.Registration.Services
{
	public class RegistrationService : IRegistrationService
	{
		private readonly ILogger<RegistrationService> _logger;
		private readonly IPublisher<IRegistrationInfo> _publisher;
		private readonly IHashCodeService<EmailHashDto> _hashCodeService;
		private readonly IUserInfoService _userInfoService;

		public RegistrationService(ILogger<RegistrationService> logger, IPublisher<IRegistrationInfo> publisher, IHashCodeService<EmailHashDto> hashCodeService, IUserInfoService userInfoService)
		{
			_logger = logger;
			_publisher = publisher;
			_hashCodeService = hashCodeService;
			_userInfoService = userInfoService;
		}

		public async ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request)
		{
			string email = request.UserName;

			string hash = _hashCodeService.New(new EmailHashDto(email));
			if (hash == null)
				return CommonGrpcResponse.Fail;

			CommonGrpcResponse createResponse = await _userInfoService.CreateUserInfoAsync(new UserInfoRegisterRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				ActivationHash = hash
			});

			if (!createResponse.IsSuccess)
				return CommonGrpcResponse.Fail;

			var recoveryInfoServiceBusModel = new RegistrationInfoServiceBusModel
			{
				Email = email,
				Hash = hash
			};

			_logger.LogDebug($"Publish into to service bus: {JsonSerializer.Serialize(recoveryInfoServiceBusModel)}");

			await _publisher.PublishAsync(recoveryInfoServiceBusModel);

			return CommonGrpcResponse.Success;
		}

		public async ValueTask<CommonGrpcResponse> ConfirmRegistrationAsync(ConfirmRegistrationGrpcRequest request)
		{
			string hash = request.Hash;

			string email = _hashCodeService.Get(hash)?.Email;
			if (email == null)
				return CommonGrpcResponse.Fail;

			_logger.LogDebug("Confirm user {email} request.", email);

			CommonGrpcResponse response = await _userInfoService.ConfirmUserInfoAsync(new UserInfoConfirmRequest
			{
				ActivationHash = hash
			});

			return CommonGrpcResponse.Result(response.IsSuccess);
		}
	}
}