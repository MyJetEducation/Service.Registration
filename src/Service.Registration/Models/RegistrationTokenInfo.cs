using System;
using System.Runtime.Serialization;

namespace Service.Registration.Models
{
	[DataContract]
	public class RegistrationTokenInfo
	{
		[DataMember(Order = 1)]
		public Guid? RegistrationUserId { get; set; }

		[DataMember(Order = 2)]
		public DateTime RegistrationTokenExpires { get; set; }
	}
}