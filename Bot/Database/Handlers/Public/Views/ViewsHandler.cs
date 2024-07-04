using Bot.Database.Types.Public.Views;
using Npgsql;

namespace Bot.Database.Handlers.Public.Views;

public class ViewsHandler(string connectionString) : BaseHandler(connectionString)
{
    /// <summary>
    /// Gets the leaderboard for messages in a specific channel.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<ChannelMessageViewRow>> GetChannelMessages(ulong channelId, int limit = 10)
    {
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM per_channel_message_view WHERE channel_id = @channel_id ORDER BY messages_sent DESC LIMIT @limit;";

        command.Parameters.AddWithValue("channel_id", channelId);
        command.Parameters.AddWithValue("limit", limit);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        List<ChannelMessageViewRow> rows = [];

        while (await reader.ReadAsync())
        {
            rows.Add(new ChannelMessageViewRow(ConnectionString, HandlersGroup, reader));
        }

        return rows;
    }
}