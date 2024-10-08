﻿using RabbitMQ.Client;

namespace ServiceBus.RabbitMQ
{
	public interface IRabbitMQPersistentConnection : IDisposable
	{
		bool IsConnected { get; }

		bool TryConnect();

		IModel CreateModel();
	}
}