using Bot.Database.Types.Public.Views;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public.Views;

public class ViewsHandler(string connectionString) : BaseHandler(connectionString)
{
    /// <summary>
    ///     Gets the leaderboard for messages in a specific channel.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<ChannelMessageViewRow>> GetChannelMessages(ulong channelId, int limit = 10)
    {
        await using var connection = await Connection();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT * FROM channel_message_view WHERE channel_id = @channel_id ORDER BY messages_sent DESC LIMIT @limit;";

        command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)channelId });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Numeric) { Value = limit });

        await using var reader = await command.ExecuteReaderAsync();
        List<ChannelMessageViewRow> rows = [];

        while (await reader.ReadAsync()) rows.Add(new ChannelMessageViewRow(ConnectionString, HandlersGroup, reader));

        return rows;
    }

    /// <inheritdoc cref="GetChannelMessages(ulong,int)" />
    public async Task<IReadOnlyList<ChannelMessageViewRow>> GetChannelMessages(DiscordChannel channel, int limit = 10)
    {
        return await GetChannelMessages(channel.Id, limit);
    }

    public async Task<IReadOnlyList<GuildMessageViewRow>> GetGuildMessages(ulong guildId, int limit = 10)
    {
        await using var connection = await Connection();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT * FROM guild_message_view WHERE guild_id = @guild_id ORDER BY messages_sent DESC LIMIT @limit;";

        command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Numeric) { Value = limit });

        await using var reader = await command.ExecuteReaderAsync();
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
        await using var connection = await Connection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM global_message_view ORDER BY messages_sent DESC LIMIT @limit;";

        command.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer) { Value = limit });

        await using var reader = await command.ExecuteReaderAsync();
        List<GlobalMessageViewRow> rows = [];

        while (await reader.ReadAsync()) rows.Add(new GlobalMessageViewRow(ConnectionString, HandlersGroup, reader));

        return rows;
    }
}