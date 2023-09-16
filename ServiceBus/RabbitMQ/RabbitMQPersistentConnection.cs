using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;

namespace ServiceBus.RabbitMQ
{
	public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
	{
		private readonly IConnectionFactory _connectionFactory;
		private readonly ILogger<RabbitMQPersistentConnection> _logger;
		private readonly int _retryCount;
		private IConnection _connection;
		private bool _disposed;
		private readonly object _syncRoot = new object();

		public bool IsConnected => _connection is not null && _connection.IsOpen && !_disposed;

        public RabbitMQPersistentConnection(
			IConnectionFactory connectionFactory, 
			ILogger<RabbitMQPersistentConnection> logger,
			int retryCount = 5)
        {
			_connectionFactory = connectionFactory;
			_logger = logger;
			_retryCount = retryCount;
		}

        public IModel CreateModel()
		{
			if (!IsConnected)
				throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");

			return _connection.CreateModel();
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;

			try
			{
				_connection.Dispose();
			}
			catch (IOException ex)
			{
				_logger.LogCritical(ex.Message);
			}
			
		}

		public bool TryConnect()
		{
			_logger.LogInformation("RabbitMQ client is trying to connect");
			
			lock (_syncRoot)
			{
				var policy = RetryPolicy.Handle<SocketException>()
					.Or<BrokerUnreachableException>()
					.WaitAndRetry(_retryCount, 
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
					(ex, time) =>
					{
						_logger.LogWarning(ex, $"RabbitMQ client could not connect after {time.TotalSeconds:n1}s {ex.Message}");
					});

				policy.Execute(() =>
				{
					_connection = _connectionFactory.CreateConnection();
				});

				if (IsConnected)
				{
					_connection.ConnectionShutdown += OnConnectionShutdown;
					_connection.CallbackException += OnCallbackException;
					_connection.ConnectionBlocked += OnConnectionBlocked;

					_logger.LogInformation($"RabbitMQ client acquired a persistent connection to {_connection.Endpoint.HostName} and is subscribed to fail");

					return true;
				}
				else
				{
					_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
					return false;
				}
				
			}
		}

		private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is shutdow. Trying to re-connect...");

			TryConnect();
		}

		private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is shutdow. Trying to re-connect...");

			TryConnect();
		}

		private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is shutdow. Trying to re-connect...");

			TryConnect();
		}
	}
}