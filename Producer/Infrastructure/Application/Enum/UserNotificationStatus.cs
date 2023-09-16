using System.ComponentModel;

namespace Producer.Infrastructure.Application.Enum
{
	[Flags]
	public enum UserNotificationStatus
	{
		[Description("Unread")]
		UnRead = 1,

		[Description("Readed")]
		Readed = 2,

		[Description("Removed")]
		Removed = 3
	}
}
