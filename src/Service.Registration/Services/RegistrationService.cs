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
using Service.UserAccount.Grpc;
using Service.UserAccount.Grpc.Models;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;

namespace Service.Registration.Services
{
	public class RegistrationService : IRegistrationService
	{
		private readonly ILogger<RegistrationService> _logger;
		private readonly IServiceBusPublisher<RegistrationInfoServiceBusModel> _publisher;
		private readonly IGrpcServiceProxy<IUserInfoService> _userInfoService;
		private readonly IEducationProgressService _progressService;
		private readonly IGrpcServiceProxy<IUserAccountService> _userProfileService;
		private readonly IEncoderDecoder _encoderDecoder;
		private readonly ISystemClock _systemClock;

		public RegistrationService(ILogger<RegistrationService> logger,
			IServiceBusPublisher<RegistrationInfoServiceBusModel> publisher,
			IGrpcServiceProxy<IUserInfoService> userInfoService,
			IEducationProgressService progressService,
			IGrpcServiceProxy<IUserAccountService> userProfileService, 
			IEncoderDecoder encoderDecoder, 
			ISystemClock systemClock)
		{
			_logger = logger;
			_publisher = publisher;
			_userInfoService = userInfoService;
			_progressService = progressService;
			_userProfileService = userProfileService;
			_encoderDecoder = encoderDecoder;
			_systemClock = systemClock;
		}

		public async ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request)
		{
			UserIdResponse userIdResponse = await CreateUserInfo(request);
			Guid? userId = userIdResponse.UserId;
			if (userId == null)
				return CommonGrpcResponse.Fail;

			bool userAccountSaveResponse = await SaveUserAccount(request, userId);
			if (!userAccountSaveResponse)
				return CommonGrpcResponse.Fail;

			string userName = request.UserName;

			string hash = GenerateRegistrationToken(userId);

			await PublishToServiceBus(userName, hash);
			
			return CommonGrpcResponse.Success;
		}

		private string GenerateRegistrationToken(Guid? userId)
		{
			int timeout = Program.ReloadedSettings(model => model.RegistrationTokenExpireMinutes).Invoke();

			return _encoderDecoder.EncodeProto(new RegistrationTokenInfo
			{
				RegistrationUserId = userId,
				RegistrationTokenExpires = _systemClock.Now.AddMinutes(timeout)
			});
		}

		private async Task<UserIdResponse> CreateUserInfo(RegistrationGrpcRequest request)
		{
			var userInfoRegisterRequest = new UserInfoRegisterRequest
			{
				UserName = request.UserName,
				Password = request.Password
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

			CommonGrpcResponse response = await _userProfileService.TryCall(service => service.SaveAccount(saveAccountGrpcRequest));
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
			string token = request.Hash;

			RegistrationTokenInfo tokenInfo = DecodeRegistrationToken(token);
			if (tokenInfo == null)
				return result;
			
			Guid? userId = tokenInfo.RegistrationUserId;
			if (tokenInfo.RegistrationTokenExpires < _systemClock.Now)
			{
				_logger.LogWarning("Token {token} for user: {userId} has expired ({date})", token, userId, tokenInfo.RegistrationTokenExpires);
				return result;
			}

			_logger.LogDebug("Confirm user registration for user {userId} with hash: {hash}.", userId, token);

			ActivateUserInfoResponse activateResponse = await _userInfoService.TryCall(service => service.ActivateUserInfoAsync(new UserInfoActivateRequest
			{
				UserId = userId
			}));

			string userName = activateResponse?.UserName;
			if (!userName.IsNullOrEmpty())
				await InitUserProgress(userId);
			else
			{
				_logger.LogError("Can't confirm user registration for user {userId}.", userId);
				return result;
			}

			result.Email = userName;

			return result;
		}

		private RegistrationTokenInfo DecodeRegistrationToken(string token)
		{
			RegistrationTokenInfo tokenInfo = null;

			try
			{
				tokenInfo = _encoderDecoder.DecodeProto<RegistrationTokenInfo>(token);
			}
			catch (Exception exception)
			{
				_logger.LogError("Can't decode registration token info ({token}), with message {message}", token, exception.Message);
			}

			return tokenInfo;
		}

		private async Task InitUserProgress(Guid? userId)
		{
			CommonGrpcResponse initResponse = await _progressService.InitProgressAsync(new InitEducationProgressGrpcRequest {UserId = userId});
			bool? initSuccess = initResponse?.IsSuccess;
			if (initSuccess != true)
				_logger.LogError("Can't init user progress for {userId}. Init return false.", userId);
		}
	}
}