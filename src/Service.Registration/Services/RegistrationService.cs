using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Core.Client.Services;
using Service.EducationProgress.Grpc;
using Service.EducationProgress.Grpc.Models;
using Service.Grpc;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.Registration.Models;
using Service.ServiceBus.Models;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;
using Service.UserProfile.Grpc;
using Service.UserProfile.Grpc.Models;

namespace Service.Registration.Services
{
	public class RegistrationService : IRegistrationService
	{
		private readonly ILogger<RegistrationService> _logger;
		private readonly IServiceBusPublisher<RegistrationInfoServiceBusModel> _publisher;
		private readonly IHashCodeService<EmailHashDto> _hashCodeService;
		private readonly IGrpcServiceProxy<IUserInfoService> _userInfoService;
		private readonly IEducationProgressService _progressService;
		private readonly IUserProfileService _userProfileService;

		public RegistrationService(ILogger<RegistrationService> logger,
			IServiceBusPublisher<RegistrationInfoServiceBusModel> publisher,
			IHashCodeService<EmailHashDto> hashCodeService,
			IGrpcServiceProxy<IUserInfoService> userInfoService,
			IEducationProgressService progressService,
			IUserProfileService userProfileService)
		{
			_logger = logger;
			_publisher = publisher;
			_userInfoService = userInfoService;
			_progressService = progressService;
			_userProfileService = userProfileService;

			_hashCodeService = hashCodeService;
			_hashCodeService.SetTimeOut(Program.ReloadedSettings(model => model.HashStoreTimeoutMinutes).Invoke());
		}

		public async ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request)
		{
			string email = request.UserName;
			if (_hashCodeService.Contains(dto => dto.Email == email))
				return CommonGrpcResponse.Success;

			string hash = _hashCodeService.New(new EmailHashDto(email));
			if (hash == null)
				return CommonGrpcResponse.Fail;

			UserIdResponse userIdResponse = await CreateUserInfo(request, hash);

			Guid? userId = userIdResponse.UserId;
			if (userId == null)
				return CommonGrpcResponse.Fail;

			await PublishToServiceBus(email, hash);

			bool userAccountSaveResponse = await SaveUserAccount(request, userId);

			return CommonGrpcResponse.Result(userAccountSaveResponse);
		}

		private async Task<UserIdResponse> CreateUserInfo(RegistrationGrpcRequest request, string hash)
		{
			var userInfoRegisterRequest = new UserInfoRegisterRequest
			{
				UserName = request.UserName,
				Password = request.Password,
				ActivationHash = hash
			};

			_logger.LogDebug($"Create user info: {JsonSerializer.Serialize(userInfoRegisterRequest)}");

			return await _userInfoService.TryCall(service => service.CreateUserInfoAsync(userInfoRegisterRequest));
		}

		private async ValueTask<bool> SaveUserAccount(RegistrationGrpcRequest request, Guid? userId)
		{
			var saveAccountGrpcRequest = new SaveAccountGrpcRequest
			{
				UserId = userId,
				FirstName = request.FirstName,
				LastName = request.LastName
			};

			_logger.LogDebug($"Saving account for user : {JsonSerializer.Serialize(saveAccountGrpcRequest)}");

			CommonGrpcResponse response = await _userProfileService.SaveAccount(saveAccountGrpcRequest);
			if (!response.IsSuccess)
				_logger.LogError("Can't save user account info for {email}.", request.UserName.Mask());

			return response.IsSuccess;
		}

		private async Task PublishToServiceBus(string email, string hash)
		{
			var recoveryInfoServiceBusModel = new RegistrationInfoServiceBusModel
			{
				Email = email,
				Hash = hash
			};

			_logger.LogDebug($"Publish into to service bus: {JsonSerializer.Serialize(recoveryInfoServiceBusModel)}");

			await _publisher.PublishAsync(recoveryInfoServiceBusModel);
		}

		public async ValueTask<ConfirmRegistrationGrpcResponse> ConfirmRegistrationAsync(ConfirmRegistrationGrpcRequest request)
		{
			var result = new ConfirmRegistrationGrpcResponse();
			string hash = request.Hash;

			string email = _hashCodeService.Get(hash)?.Email;
			if (email == null)
				return result;

			_logger.LogDebug("Confirm user registration for {email} with hash: {hash}.", email.Mask(), hash);

			CommonGrpcResponse response = await _userInfoService.TryCall(service => service.ConfirmUserInfoAsync(new UserInfoConfirmRequest
			{
				ActivationHash = hash
			}));

			bool confirmed = response.IsSuccess;
			if (confirmed)
				await InitUserProgress(email);
			else
				_logger.LogError("Can't confirm user registration for {email}.", email.Mask());

			return new ConfirmRegistrationGrpcResponse {Email = email};
		}

		private async Task InitUserProgress(string email)
		{
			UserInfoResponse userInfo = await _userInfoService.Service.GetUserInfoByLoginAsync(new UserInfoAuthRequest {UserName = email});
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