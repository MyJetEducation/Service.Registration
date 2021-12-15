namespace Service.Registration.Domain.Models
{
	public interface IRegistrationInfo
	{
		string Email { get; set; }

		string Hash { get; set; }
	}
}