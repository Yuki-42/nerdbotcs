using System.Data;
using Bot.Database.Types.Public;
using DisCatSharp;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Reactions;

public class ReactionsRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseRow(connectionString, handlersGroup, reader)
{
    // This is nullable, so we need to use the GetValueOrDefault method.
    public ulong? GuildId { get; } = GetNullableUlong(reader, "guild_id");

    public ulong? ChannelId { get; } = GetNullableUlong(reader, "channel_id");

    public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));

    public string? Emoji
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT emoji FROM reactions.reactions WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });

            dynamic? result = command.ExecuteScalar();
            return result is DBNull ? null : result;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE reactions.reactions SET emoji = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)UserId }); // TODO: Add error handling for when value is null

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Text) { Value = value });
            ExecuteNonQuery(command);
        }
    }


    public ulong? EmojiId
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT emoji_id FROM reactions.reactions WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });

            dynamic? result = command.ExecuteScalar();
            return result is DBNull ? null : (ulong?)result;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE reactions.reactions SET emoji_id = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Numeric) { Value = value }); // TODO: Add error handling for when value is null
            ExecuteNonQuery(command);
        }
    }

    public string? Type
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT type FROM reactions.reactions WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });
            return (string?)command.ExecuteScalar();
        }
    }

    public bool TryGetEmoji(DiscordClient client, out DiscordEmoji? emojiOut)
    {
        try
        {
            // Get the type of the emoji
            string? type = Type;

            if (type is null)
            {
                emojiOut = null;
                return false;
            }

            switch (type)
            {
                case EmojiTypes.Unicode:
                    return DiscordEmoji.TryFromUnicode(client, Emoji!, out emojiOut);
                case EmojiTypes.Discord:
                    return DiscordEmoji.TryFromName(client, Emoji!, out emojiOut);
                case EmojiTypes.Guild:
                    return DiscordEmoji.TryFromGuildEmote(client, (ulong)EmojiId!, out emojiOut);
                default:
                    emojiOut = null;
                    return false;
            }
        }
        catch
        {
            emojiOut = null;
            return false;
        }
    }

    public async Task Delete()
    {
        await using NpgsqlConnection connection = await GetConnectionAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM reactions.reactions WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });

        await command.ExecuteNonQueryAsync();
    }

    public async Task<UsersRow> GetUser()
    {
        return (await HandlersGroup.Public.Users.Get(UserId))!;
    }

    public async Task<ChannelsRow?> GetChannel()
    {
        return ChannelId is null ? null : await HandlersGroup.Public.Channels.Get(ChannelId.Value);
    }

    public async Task<GuildsRow?> GetGuild()
    {
        return GuildId is null ? null : await HandlersGroup.Public.Guilds.Get(GuildId.Value);
    }
}