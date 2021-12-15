using System;

namespace Service.Registration.Postgres.Models
{
    public class RegisterLogModel
    {
        public int Id { get; set; }
        public string TraderId { get; set; }

        public string Email { get; set; }

        public string RegistrationCountryCode { get; set; }

        public string Phone { get; set; }

        public string Ip { get; set; }

        public string UserAgent { get; set; }

        public string Owner { get; set; }

        public string RedirectedFrom { get; set; }

        public string LandingPageId { get; set; }

        public string Language { get; set; }

        public string CountryCodeByIp { get; set; }

        public DateTime Registered { get; set; }
    }
}