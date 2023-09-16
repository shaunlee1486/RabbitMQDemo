using Autofac;
using Producer.Configuration;
using Producer.Infrastructure.Application.Implementation;
using Producer.Infrastructure.Application.Interfaces;
using Producer.Infrastructure.Application.Provider;
using RabbitMQ.Client;
using ServiceBus.Abstraction;
using ServiceBus.Event.Core;
using ServiceBus.RabbitMQ;

namespace Producer.Ioc
{
	public static class IocConfig
	{
		public static void Register(IServiceCollection services, IConfiguration configuration)
		{
			//To use TemplateViewProvider you have to add MVC in Startup.cs class
			services.AddTransient<TemplateViewProvider>();
			services.AddTransient<IUserEmailService, UserEmailService>();

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

			services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManager>();
		}
	}
}
