using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Service.Grpc;
using Service.Registration.Grpc;

namespace Service.Registration.Client
{
    [UsedImplicitly]
    public class RegistrationClientFactory: GrpcClientFactory
    {
        public RegistrationClientFactory(string grpcServiceUrl, ILogger logger) : base(grpcServiceUrl, logger)
        {
        }

        public IGrpcServiceProxy<IRegistrationService> GetRegistrationService() => CreateGrpcService<IRegistrationService>();
    }
}
