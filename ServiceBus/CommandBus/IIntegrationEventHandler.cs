using ServiceBus.Event.Core;

namespace ServiceBus.CommandBus
{
	public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
	{
		Task Handle(T @event);
	}
}