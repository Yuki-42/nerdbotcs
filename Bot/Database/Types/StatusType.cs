using DisCatSharp.Entities;

namespace Bot.Database.Types;

public static class StatusType
{
	private const int Playing = 0;
	private const int Streaming = 1;
	private const int Listening = 2;
	private const int Watching = 3;
	private const int Custom = 4;
	private const int Competing = 5;

	public static DiscordActivity GetActivityType(int type, string status)
	{
		return type switch
		{
			Playing => new DiscordActivity
			{
				ActivityType = ActivityType.Playing,
				Name = status
			},
			Streaming => new DiscordActivity
			{
				ActivityType = ActivityType.Streaming,
				Name = status
			},
			Listening => new DiscordActivity
			{
				ActivityType = ActivityType.ListeningTo,
				Name = status
			},
			Watching => new DiscordActivity
			{
				ActivityType = ActivityType.Watching,
				Name = status
			},
			Custom => new DiscordActivity
			{
				ActivityType = ActivityType.Custom,
				Name = status
			},
			Competing => new DiscordActivity
			{
				ActivityType = ActivityType.Competing,
				Name = status
			},
			_ => new DiscordActivity
			{
				ActivityType = ActivityType.Playing,
				Name = status
			}
		};
	}
}