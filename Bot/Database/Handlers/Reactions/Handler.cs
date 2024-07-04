using System.Data;
using Bot.Database.Types;
using Bot.Database.Types.Public;
using Bot.Database.Types.Reactions;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Reactions;

public class Handler(string connectionString) : BaseHandler(connectionString)
{
    public async Task<ReactionsRow?> Get(Guid id)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM reactions.reactions WHERE id = @id";
        command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = id });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new ReactionsRow(ConnectionString, HandlersGroup, reader);
    }

    public async Task<bool> Exists(string emoji, ulong userId, ulong? guildId = null, ulong? channelId = null)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM reactions.reactions WHERE emoji = @emoji AND user_id = @user_id";
        command.Parameters.Add(new NpgsqlParameter("emoji", NpgsqlDbType.Text) { Value = emoji });
        command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)userId });

        // I cannot be bothered to make the rest of this work in SQL so I will do it in C#.

        await using NpgsqlDataReader reader = await ExecuteReader(command);

        while (await reader.ReadAsync())
        {
            ReactionsRow reaction = new(ConnectionString, HandlersGroup, reader);
            if (guildId != null && reaction.GuildId != guildId) continue;
            if (channelId != null && reaction.ChannelId != channelId) continue;
            return true;
        }

        return false;
    }

    public async Task<ReactionsRow> New(string emoji, ulong appliesTo, ulong? guildId = null, ulong? channelId = null)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO reactions.reactions (emoji, emoji_id, user_id, guild_id, channel_id, type) VALUES (@emoji, @emoji_id, @user_id, @guild_id, @channel_id, @type) RETURNING id;";
        command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)appliesTo });
        command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = guildId == null ? DBNull.Value : (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = channelId == null ? DBNull.Value : (long)channelId });

        string emojiType;

        // Get type using regex matching
        if (RegularExpressions.UnicodeEmoji.Match(emoji).Success)
        {
            emojiType = EmojiTypes.Unicode;
            command.Parameters.Add(new NpgsqlParameter("emoji", NpgsqlDbType.Text) { Value = emoji });
            command.Parameters.Add(new NpgsqlParameter("emoji_id", NpgsqlDbType.Numeric) { Value = DBNull.Value });
        }
        else if (RegularExpressions.DiscordEmoji.Match(emoji).Success)
        {
            emojiType = EmojiTypes.Discord;
            command.Parameters.Add(new NpgsqlParameter("emoji", NpgsqlDbType.Text) { Value = emoji });
            command.Parameters.Add(new NpgsqlParameter("emoji_id", NpgsqlDbType.Numeric) { Value = DBNull.Value });
        }
        else if (RegularExpressions.GuildEmoji.Match(emoji).Success)
        {
            emojiType = EmojiTypes.Guild;
            command.Parameters.Add(new NpgsqlParameter("emoji", NpgsqlDbType.Text) { Value = DBNull.Value });
            command.Parameters.Add(new NpgsqlParameter("emoji_id", NpgsqlDbType.Numeric) { Value = (long)RegularExpressions.ExtractId(emoji) });
        }
        else
        {
            emojiType = EmojiTypes.Unknown;
            command.Parameters.Add(new NpgsqlParameter("emoji", NpgsqlDbType.Text) { Value = emoji });
            command.Parameters.Add(new NpgsqlParameter("emoji_id", NpgsqlDbType.Numeric) { Value = DBNull.Value });
        }

        Console.WriteLine(emojiType);
        command.Parameters.Add(new NpgsqlParameter("type", NpgsqlDbType.Text) { Value = emojiType });
        Console.WriteLine(emojiType);

        try
        {
            await using NpgsqlDataReader reader = await ExecuteReader(command);
            reader.Read();
            return (await Get(reader.GetGuid(0)))!;
        }
        catch (Exception error)
        {
            Console.WriteLine($"Failed to create reaction: {error.Message}");
            throw;
        }
    }

    public async Task<ReactionsRow> New(string emoji, UsersRow appliesTo, GuildsRow? guild = null, ChannelsRow? channel = null)
    {
        return await New(emoji, appliesTo.Id, guild?.Id, channel?.Id);
    }

    public async Task<IEnumerable<ReactionsRow>> GetReactions(ulong user, ulong? guildId = null, ulong? channelId = null)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM reactions.reactions WHERE user_id = @user_id;";
        command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)user });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        List<ReactionsRow> reactions = [];
        while (await reader.ReadAsync()) reactions.Add(new ReactionsRow(ConnectionString, HandlersGroup, reader));

        // Now filter the reactions by guild and channel.
        if (guildId != null) reactions = reactions.Where(x => x.GuildId == guildId).ToList();

        if (channelId != null) reactions = reactions.Where(x => x.ChannelId == channelId).ToList();

        return reactions;
    }
}