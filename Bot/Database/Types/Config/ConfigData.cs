﻿using System.Data;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Config;

public class ConfigData(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseType(connectionString, handlersGroup, reader)
{
    /// <summary>
    ///     Configuration string key.
    /// </summary>
    public string Key { get; } = reader.GetString(reader.GetOrdinal("key"));

    /// <summary>
    ///     Configuration string value.
    /// </summary>
    public string Value
    {
        get
        {
            using NpgsqlConnection connection = GetConnectionAsync().Result;
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT value FROM config.data WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = Id });

            return (command.ExecuteScalar() as string)!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE config.data SET value = @value WHERE id = @id;";
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = Id }); // This style is probably going to cause errors, best to swap over to using AddWithValue if it does.

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Text) { Value = value });
            ExecuteNonQuery(command);
        }
    }
}