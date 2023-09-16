using ServiceBus.Event.Core;

namespace ServiceBus.Event
{
	public class EmailIntegrationEvent : IntegrationEvent
	{
		public string Subject { get; set; }
		public List<string> To { get; set; }
		public List<string> Cc { get; set; } = new List<string>();
		public List<string> Bcc { get; set; } = new List<string>();
		public string Body { get; set; }
	}
}