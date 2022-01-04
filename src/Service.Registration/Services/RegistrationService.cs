using System.Text.Json;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Core.Domain.Extensions;
using Service.Core.Domain.Models;
using Service.Core.Grpc.Models;
using Service.EducationProgress.Grpc;
using Service.EducationProgress.Grpc.Models;
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
		private readonly IEducationProgressService _progressService;

		public RegistrationService(ILogger<RegistrationService> logger, IPublisher<IRegistrationInfo> publisher, IHashCodeService<EmailHashDto> hashCodeService, IUserInfoService userInfoService, IEducationProgressService progressService)
		{
			_logger = logger;
			_publisher = publisher;
			_hashCodeService = hashCodeService;
			_userInfoService = userInfoService;
			_progressService = progressService;
		}

		public async ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request)
		{
			string email = request.UserName;

			string hash = _hashCodeService.New(new EmailHashDto(email));
			if (hash == null)
				return CommonGrpcResponse.Fail;

			var userInfoRegisterRequest = new UserInfoRegisterRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				ActivationHash = hash
			};

			_logger.LogDebug($"Create user info: {JsonSerializer.Serialize(userInfoRegisterRequest)}");

			CommonGrpcResponse createResponse = await _userInfoService.CreateUserInfoAsync(userInfoRegisterRequest);

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

			_logger.LogDebug("Confirm user registration for {email} with hash: {hash}.", email, hash);

			CommonGrpcResponse response = await _userInfoService.ConfirmUserInfoAsync(new UserInfoConfirmRequest
			{
				ActivationHash = hash
			});

			bool confirmed = response.IsSuccess;
			if (confirmed)
				await InitUserProgress(email);

			return CommonGrpcResponse.Result(confirmed);
		}

		private async Task InitUserProgress(string email)
		{
			UserInfoResponse userInfo = await _userInfoService.GetUserInfoByLoginAsync(new UserInfoAuthRequest {UserName = email});
			if (userInfo == null)
				_logger.LogError("Can't init user progress for {email}. No info for user retrieved", email.Mask());
			else
			{
				CommonGrpcResponse initResponse = await _progressService.InitProgressAsync(new InitEducationProgressGrpcRequest {UserId = userInfo.UserInfo.UserId});
				bool? initSuccess = initResponse?.IsSuccess;
				if (initSuccess != true)
					_logger.LogError("Can't init user progress for {email}. Init return false.", email.Mask());
			}
		}
	}
}