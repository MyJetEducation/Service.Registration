using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Registration.Grpc;

namespace Service.Registration.Client
{
    [UsedImplicitly]
    public class RegistrationClientFactory: MyGrpcClientFactory
    {
        public RegistrationClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
