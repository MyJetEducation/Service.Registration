using System;

namespace Service.Registration.Postgres.Models
{
    public class ExternalDataEntity
    {
        public string Id { get; set; }
        public string TraderId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public static ExternalDataEntity Create(string traderId, string key, string value)
        {
            return new ExternalDataEntity
            {
                Id = Guid.NewGuid().ToString(),
                TraderId = traderId,
                Key = key,
                Value = value
            };
        }
    }
}