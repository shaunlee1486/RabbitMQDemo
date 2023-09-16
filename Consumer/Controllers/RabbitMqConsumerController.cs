using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Consumer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[SwaggerTag("Consumer push mes to RabbitMq")]
	public class RabbitMqConsumerController : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return Ok("Starting service");
		}
	}
}
