namespace Consumer.Infrastructure.Services
{
	public interface IEmailService
	{
		Task SendEmailAsync(string subject,
			string body,
			IEnumerable<string> toAddresses,
			IEnumerable<string> ccAddresses = null,
			IEnumerable<string> bccAddresses = null,
			bool isBodyHtml = true);
	}
}
