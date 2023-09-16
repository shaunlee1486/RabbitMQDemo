using ServiceBus.CommandBus;
using ServiceBus.Event.Core;

namespace ServiceBus.RabbitMQ
{
	public interface IEventBusRabbitMQ
	{
		void Publish(IntegrationEvent @event);

		//TH: handler event
		void Subscribe<T, TH>()
			where T : IntegrationEvent
			where TH : IIntegrationEventHandler<T>;

		void Unsubscribe<T, TH>()
			where TH : IIntegrationEventHandler<T>
			where T : IntegrationEvent;
	}
}