using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Registration.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Registration.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Registration.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Registration.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("Registration.ServiceBusWriter")]
        public string ServiceBusWriter { get; set; }

        [YamlProperty("Registration.UserInfoCrudServiceUrl")]
        public string UserInfoCrudServiceUrl { get; set; }

        [YamlProperty("Registration.EducationProgressServiceUrl")]
        public string EducationProgressServiceUrl { get; set; }

        [YamlProperty("Registration.UserAccountServiceUrl")]
        public string UserAccountServiceUrl { get; set; }

        [YamlProperty("Registration.HashStoreTimeoutMinutes")]
        public int HashStoreTimeoutMinutes { get; set; }
    }
}
