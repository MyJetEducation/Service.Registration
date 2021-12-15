namespace Service.Registration.Models
{
	public class EmailHashDto
	{
		public EmailHashDto(string email) => Email = email;

		public string Email { get; set; }
	}
}