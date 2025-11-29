using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Bot.Database.Types.Public.Views;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ChannelMessageViewRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
	: BaseViewRow(connectionString, handlersGroup)
{
	/// <summary>
	///  Guild id.
	/// </summary>
	public ulong GuildId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));

	/// <summary>
	///  Channel id.
	/// </summary>
	public ulong ChannelId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id"));

	/// <summary>
	///  User id.
	/// </summary>
	public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));

	/// <summary>
	///  The guild.
	/// </summary>
	public GuildsRow Guild => HandlersGroup.Public.Guilds.Get(GuildId).Result!;

	/// <summary>
	///  The channel.
	/// </summary>
	public ChannelsRow Channel => HandlersGroup.Public.Channels.Get(ChannelId).Result!;

	/// <summary>
	///  The user.
	/// </summary>
	public UsersRow User => HandlersGroup.Public.Users.Get(UserId).Result!;

	/// <summary>
	///  The number of messages sent by the user in the channel.
	/// </summary>
	public long MessagesSent { get; } = reader.GetInt64(reader.GetOrdinal("messages_sent"));
}