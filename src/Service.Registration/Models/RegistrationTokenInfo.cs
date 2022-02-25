using System;

namespace Service.Registration.Models
{
	public class RegistrationTokenInfo
	{
		public Guid? RegistrationUserId { get; set; }
		
		public DateTime RegistrationTokenExpires { get; set; }
	}
}