using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class ChannelsUsersRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
    : BaseRow(connectionString, handlersGroup, reader)
{
    public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));
    public ulong ChannelId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("channel_id"));

    public bool MessageTracking
    {
        get
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT message_tracking FROM public.channels_users WHERE user_id = @user_id AND channel_id = @channel_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)ChannelId });

            return (bool)command.ExecuteScalar()!;
        }
        set
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText =
                "UPDATE public.channels_users SET message_tracking = @value WHERE user_id = @user_id AND channel_id = @channel_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)ChannelId });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
            ExecuteNonQuery(command);
        }
    }

    public long MessagesSent
    {
        get
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT messages_sent FROM public.channels_users WHERE user_id = @user_id AND channel_id = @channel_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)ChannelId });

            return (long)command.ExecuteScalar()!;
        }
        set
        {
            using var connection = GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText =
                "UPDATE public.channels_users SET messages_sent = @value WHERE user_id = @user_id AND channel_id = @channel_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlDbType.Numeric) { Value = (long)ChannelId });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Numeric) { Value = value });
            ExecuteNonQuery(command);
        }
    }

    public async Task<UsersRow?> GetUser()
    {
        return await HandlersGroup.Public.Users.Get(UserId) ?? throw new MissingMemberException();
    }

    public async Task<ChannelsRow?> GetChannel()
    {
        return await HandlersGroup.Public.Channels.Get(ChannelId, null) ?? throw new MissingMemberException();
    }

    public async Task Delete()
    {
        await using var connection = await GetConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM public.channels_users WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = Id });

        await command.ExecuteNonQueryAsync();
    }
}