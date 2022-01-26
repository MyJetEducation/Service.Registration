using System;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Core.Client.Services;
using Service.EducationProgress.Grpc;
using Service.EducationProgress.Grpc.Models;
using Service.Registration.Domain.Models;
using Service.Registration.Grpc;
using Service.Registration.Grpc.Models;
using Service.Registration.Models;
using Service.UserInfo.Crud.Grpc;
using Service.UserInfo.Crud.Grpc.Models;
using Service.UserProfile.Grpc;
using Service.UserProfile.Grpc.Models;

namespace Service.Registration.Services
{
	public class RegistrationService : IRegistrationService
	{
		private readonly ILogger<RegistrationService> _logger;
		private readonly IPublisher<RegistrationInfoServiceBusModel> _publisher;
		private readonly IHashCodeService<EmailHashDto> _hashCodeService;
		private readonly IUserInfoService _userInfoService;
		private readonly IEducationProgressService _progressService;
		private readonly IUserProfileService _userProfileService;

		public RegistrationService(ILogger<RegistrationService> logger,
			IPublisher<RegistrationInfoServiceBusModel> publisher,
			IHashCodeService<EmailHashDto> hashCodeService,
			IUserInfoService userInfoService,
			IEducationProgressService progressService,
			IUserProfileService userProfileService)
		{
			_logger = logger;
			_publisher = publisher;
			_hashCodeService = hashCodeService;
			_userInfoService = userInfoService;
			_progressService = progressService;
			_userProfileService = userProfileService;
		}

		public async ValueTask<CommonGrpcResponse> RegistrationAsync(RegistrationGrpcRequest request)
		{
			string email = request.UserName;

			string hash = _hashCodeService.New(new EmailHashDto(email));
			if (hash == null)
				return CommonGrpcResponse.Fail;

			UserIdResponse userIdResponse = await CreateUserInfo(request, hash);

			Guid? userId = userIdResponse.UserId;
			if (userId == null)
				return CommonGrpcResponse.Fail;

			await SaveUserAccount(request.FullName, userId);

			await PublishToServiceBus(email, hash);

			return CommonGrpcResponse.Success;
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

			return await _userInfoService.CreateUserInfoAsync(userInfoRegisterRequest);
		}

		private async Task SaveUserAccount(string fullName, Guid? userId)
		{
			string[] nameParts = fullName.Split(" ");
			if (nameParts.Length != 2)
			{
				_logger.LogError($"Can't save account for userId: {userId}, invalid fullname: {fullName}.");
				return;
			}

			var saveAccountGrpcRequest = new SaveAccountGrpcRequest
			{
				UserId = userId,
				FirstName = nameParts[0],
				LastName = nameParts[1]
			};

			_logger.LogDebug($"Saving account for user : {JsonSerializer.Serialize(saveAccountGrpcRequest)}");

			await _userProfileService.SaveAccount(saveAccountGrpcRequest);
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

			_logger.LogDebug("Confirm user registration for {email} with hash: {hash}.", email, hash);

			CommonGrpcResponse response = await _userInfoService.ConfirmUserInfoAsync(new UserInfoConfirmRequest
			{
				ActivationHash = hash
			});

			bool confirmed = response.IsSuccess;
			if (confirmed)
				await InitUserProgress(email);

			return new ConfirmRegistrationGrpcResponse {Email = email};
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