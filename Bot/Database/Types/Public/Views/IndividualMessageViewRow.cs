using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Bot.Database.Types.Public.Views;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class IndividualMessageViewRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
	: BaseViewRow(connectionString, handlersGroup)
{
	/// <summary>
	/// User id.
	/// </summary>
	public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));

	/// <summary>
	/// Guild id.
	/// </summary>
	public ulong GuildId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));

	/// <summary>
	/// Channel id.
	/// </summary>
	public ulong ChannelId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id"));

	/// <summary>
	/// Messages sent by the user.
	/// </summary>
	public long MessagesSent { get; } = reader.GetInt64(reader.GetOrdinal("messages_sent"));
}