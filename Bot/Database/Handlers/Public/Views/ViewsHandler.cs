using System.Diagnostics.CodeAnalysis;
using Bot.Database.Types.Public.Views;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public.Views;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ViewsHandler(string connectionString) : BaseHandler(connectionString)
{
	public async Task<IndividualMessageViewRow?> GetGlobalIndividualMessages(ulong userId)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();

		command.CommandText = "SELECT * FROM individual_message_view WHERE user_id = @user_id;";
		command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)userId });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		if (!await reader.ReadAsync()) return null;

		return new IndividualMessageViewRow(ConnectionString, HandlersGroup, reader);
	}

	public async Task<IndividualMessageViewRow?> GetGuildIndividualMessages(ulong userId, ulong guildId)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();

		command.CommandText = "SELECT * FROM individual_message_view WHERE user_id = @user_id AND guild_id = @guild_id;";

		command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)userId });
		command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)guildId });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		if (!await reader.ReadAsync()) return null;

		return new IndividualMessageViewRow(ConnectionString, HandlersGroup, reader);
	}

	public async Task<IndividualMessageViewRow?> GetGuildIndividualMessages(ulong userId, DiscordGuild guild)
	{
		return await GetGuildIndividualMessages(userId, guild.Id);
	}

	public async Task<IndividualMessageViewRow?> GetChannelIndividualMessages(ulong userId, ulong channelId)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();

		command.CommandText = "SELECT * FROM individual_message_view WHERE user_id = @user_id AND channel_id = @channel_id;";

		command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)userId });
		command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)channelId });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		if (!await reader.ReadAsync()) return null;

		return new IndividualMessageViewRow(ConnectionString, HandlersGroup, reader);
	}

	public async Task<IndividualMessageViewRow?> GetChannelIndividualMessages(ulong userId, DiscordChannel channel)
	{
		return await GetChannelIndividualMessages(userId, channel.Id);
	}

	/// <summary>
	///  Gets the leaderboard for messages in a specific channel.
	/// </summary>
	/// <returns></returns>
	// ReSharper disable once MemberCanBePrivate.Global
	public async Task<IReadOnlyList<ChannelMessageViewRow>> GetChannelMessages(ulong channelId, int limit = 10)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();
		command.CommandText =
			"SELECT * FROM channel_message_view WHERE channel_id = @channel_id ORDER BY messages_sent DESC LIMIT @limit;";

		command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)channelId });
		command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Numeric) { Value = limit });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		List<ChannelMessageViewRow> rows = [];

		while (await reader.ReadAsync()) rows.Add(new ChannelMessageViewRow(ConnectionString, HandlersGroup, reader));

		return rows;
	}

	/// <inheritdoc cref="GetChannelMessages(ulong,int)" />
	public async Task<IReadOnlyList<ChannelMessageViewRow>> GetChannelMessages(DiscordChannel channel, int limit = 10)
	{
		return await GetChannelMessages(channel.Id, limit);
	}

	// ReSharper disable once MemberCanBePrivate.Global
	public async Task<IReadOnlyList<GuildMessageViewRow>> GetGuildMessages(ulong guildId, int limit = 10)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();
		command.CommandText =
			"SELECT * FROM guild_message_view WHERE guild_id = @guild_id ORDER BY messages_sent DESC LIMIT @limit;";

		command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)guildId });
		command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Numeric) { Value = limit });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		List<GuildMessageViewRow> rows = [];

		while (await reader.ReadAsync()) rows.Add(new GuildMessageViewRow(ConnectionString, HandlersGroup, reader));

		return rows;
	}

	/// <inheritdoc cref="GetGuildMessages(ulong,int)" />
	public async Task<IReadOnlyList<GuildMessageViewRow>> GetGuildMessages(DiscordGuild guild, int limit = 10)
	{
		return await GetGuildMessages(guild.Id, limit);
	}

	public async Task<IReadOnlyList<GlobalMessageViewRow>> GetGlobalMessages(int limit = 10)
	{
		await using NpgsqlConnection connection = await Connection();
		await using NpgsqlCommand command = connection.CreateCommand();
		command.CommandText = "SELECT * FROM global_message_view ORDER BY messages_sent DESC LIMIT @limit;";

		command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

		await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
		List<GlobalMessageViewRow> rows = [];

		while (await reader.ReadAsync()) rows.Add(new GlobalMessageViewRow(ConnectionString, HandlersGroup, reader));

		return rows;
	}
}