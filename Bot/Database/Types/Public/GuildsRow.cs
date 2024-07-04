using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class GuildsRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseRow(connectionString, handlersGroup, reader)
{
    /// <summary>
    ///     User's discord id.
    /// </summary>
    public new ulong Id { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("id"));

    /// <summary>
    ///     Guild name.
    /// </summary>
    public string Name
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM public.guilds WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            return (command.ExecuteScalar() as string)!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.guilds SET name = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Text) { Value = value });
            ExecuteNonQuery(command);
        }
    }


    /// <summary>
    ///     If the guild has message tracking enabled.
    /// </summary>
    public bool MessageTracking
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT message_tracking FROM public.guilds WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            return (bool)command.ExecuteScalar()!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.guilds SET message_tracking = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
            ExecuteNonQuery(command);
        }
    }
}