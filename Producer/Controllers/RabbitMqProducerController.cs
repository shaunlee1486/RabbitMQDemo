using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.Infrastructure.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Producer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[SwaggerTag("Producer push mes to RabbitMq")]
	public class RabbitMqProducerController : ControllerBase
	{
		private readonly IUserEmailService _emailService;

		public RabbitMqProducerController(IUserEmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpGet]
		public async Task<IActionResult> SendEmailAsync()
		{
			await _emailService.SendEmailAsync();
			return Ok("Success!!!");
		}
	}
}
