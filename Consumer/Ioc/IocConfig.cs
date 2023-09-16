using Autofac;
using Consumer.Infrastructure.Configurations;
using Consumer.Infrastructure.Handlers;
using Consumer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using ServiceBus.Abstraction;
using ServiceBus.Event.Core;
using ServiceBus.RabbitMQ;

namespace Consumer.Ioc
{
	public static class IocConfig
	{
		public static void Register(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<SmtpEmailConfiguation>(configuration.GetSection("SmtpEmailConfiguation"));
			services.AddTransient<IEmailService, EmailService>();

			RegisterEventBus(services, configuration);
		}

		private static void RegisterEventBus(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<RabitMQConfiguration>(configuration.GetSection("RabitMQConfiguration"));

			services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
			{
				var rabitMQConfiguration = new RabitMQConfiguration();
				configuration.GetSection("RabitMQConfiguration").Bind(rabitMQConfiguration);

				var logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();

				var factory = new ConnectionFactory()
				{
					HostName = rabitMQConfiguration.HostName,
				};

				if (!string.IsNullOrEmpty(rabitMQConfiguration.UserName))
					factory.UserName = rabitMQConfiguration.UserName;

				if (!string.IsNullOrEmpty(rabitMQConfiguration.Password))
					factory.Password = rabitMQConfiguration.Password;

				if (!string.IsNullOrEmpty(rabitMQConfiguration.VirtualHost))
					factory.VirtualHost = rabitMQConfiguration.VirtualHost;

				if (rabitMQConfiguration.Port.HasValue)
					factory.Port = rabitMQConfiguration.Port.Value;

				return new RabbitMQPersistentConnection(factory, logger, rabitMQConfiguration.RetryCount);
			});

			services.AddSingleton<IEventBusRabbitMQ, EventBusRabbitMQ>(sp =>
			{
				var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
				var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
				var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
				var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

				var rabitMQConfiguration = new RabitMQConfiguration();
				configuration.GetSection("RabitMQConfiguration").Bind(rabitMQConfiguration);

				return new EventBusRabbitMQ(rabbitMQPersistentConnection,
					logger, iLifetimeScope, eventBusSubcriptionsManager,
					queueName: rabitMQConfiguration.QueueName,
					exchangetype: rabitMQConfiguration.Exchangetype,
					exchangeName: rabitMQConfiguration.ExchangeName,
					retryCount: rabitMQConfiguration.RetryCount);
			});

			services.AddTransient<EmailIntegrationHandler>();

			// EventBusSubscriptionsManager
			services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManager>();
		}
	}
}
