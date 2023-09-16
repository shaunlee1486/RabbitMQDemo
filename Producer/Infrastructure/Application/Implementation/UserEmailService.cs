using Consumer.Infrastructure.Constants;
using Producer.Infrastructure.Application.Interfaces;
using Producer.Infrastructure.Application.Provider;
using ServiceBus.Event;
using ServiceBus.RabbitMQ;

namespace Producer.Infrastructure.Application.Implementation
{
	public class UserEmailService : IUserEmailService
	{
		private readonly TemplateViewProvider _templateViewProvider;
		private readonly IEventBusRabbitMQ _eventBusRabbitMQ;

		public UserEmailService(TemplateViewProvider templateViewProvider,
			IEventBusRabbitMQ eventBusRabbitMQ)
		{
			_templateViewProvider = templateViewProvider;
			_eventBusRabbitMQ = eventBusRabbitMQ;
		}

		public async Task SendEmailAsync()
		{

			_eventBusRabbitMQ.Publish(new EmailIntegrationEvent
			{
				To = new List<string> { "shaun@gmail.com" },
				Subject = await _templateViewProvider.GetTemplateBody(TemplateConstant.Email.Subject.SampleSubject, new { CustomerName = "TranDong", FrontendLink = "http://rabbitmq.com" }),
				Body = await _templateViewProvider.GetTemplateBody(TemplateConstant.Email.Body.SampleBody, new { CustomerName = "TranDong", FrontendLink = "http://rabbitmq.com" }),
			});

		}
	}
}
