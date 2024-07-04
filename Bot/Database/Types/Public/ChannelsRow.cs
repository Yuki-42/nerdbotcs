using System.Data;
using DisCatSharp.Enums;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class ChannelsRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseRow(connectionString, handlersGroup, reader)
{
    /// <summary>
    ///     User's discord id.
    /// </summary>
    public new ulong Id { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("id"));

    /// <summary>
    ///     Guild id.
    /// </summary>
    public ulong GuildId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));

    /// <summary>
    ///     If the channel has message tracking enabled.
    /// </summary>
    public bool MessageTracking
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT message_tracking FROM public.channels WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            return (bool)command.ExecuteScalar()!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.channels SET message_tracking = @value WHERE id = @id;";

            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });
            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
            ExecuteNonQuery(command);
        }
    }
    
    public async Task Delete()
    {
        await using NpgsqlConnection connection = await GetConnectionAsync();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM public.channels WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", DbType.VarNumeric) { Value = Id });

        await command.ExecuteNonQueryAsync();
    }

    public string Name
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM public.channels WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            return (command.ExecuteScalar() as string)!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.channels SET name = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Text) { Value = value });
            ExecuteNonQuery(command);
        }
    }

    public ChannelType Type
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT type FROM public.channels WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            // Convert the string to ChannelType using TryParse
            ChannelType type;

            return Enum.TryParse((command.ExecuteScalar() as string)!, out type) ? type : ChannelType.Unknown;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.channels SET type = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Text) { Value = value.ToString() });
            ExecuteNonQuery(command);
        }
    }

    /// <summary>
    ///     Associated guild object.
    /// </summary>
    /// <returns>Guild</returns>
    public async Task<GuildsRow?> GetGuild()
    {
        return await HandlersGroup.Public.Guilds.Get(GuildId);
    }
}