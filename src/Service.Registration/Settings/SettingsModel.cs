﻿using MyJetWallet.Sdk.Service;
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
    }
}
