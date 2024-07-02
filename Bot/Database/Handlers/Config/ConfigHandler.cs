using System.Data;
using System.Data.Common;
using Bot.Database.Types.Config;
using Npgsql;

namespace Bot.Database.Handlers.Config;

public class ConfigHandler(string connectionString) : BaseHandler(connectionString)
{
    public async Task<ConfigData> Get(Guid id)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM config.data WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", DbType.Guid) { Value = id });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        await reader.ReadAsync();

        return new ConfigData(ConnectionString, HandlersGroup, reader);
    }

    public async Task<ConfigData> Get(string key)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM config.data WHERE key = @key;";
        command.Parameters.Add(new NpgsqlParameter("key", DbType.String) { Value = key });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        await reader.ReadAsync();

        return new ConfigData(ConnectionString, HandlersGroup, reader);
    }
}