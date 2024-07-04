using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class GuildsUsersRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseRow(connectionString, handlersGroup, reader)
{
    public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));
    public ulong GuildId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));

    public bool MessageTracking
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT message_tracking FROM public.guilds_users WHERE user_id = @user_id AND guild_id = @guild_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)GuildId });

            return (bool)command.ExecuteScalar()!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.guilds_users SET message_tracking = @value WHERE user_id = @user_id AND guild_id = @guild_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)GuildId });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
            ExecuteNonQuery(command);
        }
    }

    public async Task<UsersRow?> GetUser()
    {
        return await HandlersGroup.Public.Users.Get(UserId) ?? throw new MissingMemberException();
    }

    public async Task<GuildsRow?> GetGuild()
    {
        return await HandlersGroup.Public.Guilds.Get(GuildId) ?? throw new MissingMemberException();
    }
}