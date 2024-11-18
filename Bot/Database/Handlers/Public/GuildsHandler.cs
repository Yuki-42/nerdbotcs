using Bot.Database.Types.Public;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public;

public class GuildsHandler(string connectionString) : BaseHandler(connectionString)
{
    /// <summary>
    ///     Get a guild from the database.
    /// </summary>
    /// <param name="id">Guild ID</param>
    /// <returns>Guild.</returns>
    public async Task<GuildsRow?> Get(ulong id)
    {
        // Get a new connection
        await using NpgsqlConnection? connection = await Connection();
        await using NpgsqlCommand? command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.guilds WHERE id = @id";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        await using NpgsqlDataReader? reader = await ExecuteReader(command);
        return !reader.Read() ? null : new GuildsRow(ConnectionString, HandlersGroup, reader);
    }

    public async Task<GuildsRow> Get(ulong id, string name)
    {
        // Check if the guild already exists
        GuildsRow? guild = await Get(id);
        if (guild != null) return guild;

        // Create a new guild
        // Get a new connection
        await using NpgsqlConnection? connection = await Connection();
        await using NpgsqlCommand? command = connection.CreateCommand();
        command.CommandText = "INSERT INTO public.guilds (id, name) VALUES (@id, @name)";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });

        await ExecuteNonQuery(command);
        return await Get(id)! ?? throw new MissingMemberException();
    }

    public async Task<GuildsRow> Get(DiscordGuild guild)
    {
        return await Get(guild.Id, guild.Name);
    }

    public async Task<IReadOnlyList<GuildsRow>> GetAll()
    {
        // Get a new connection
        await using NpgsqlConnection? connection = await Connection();
        await using NpgsqlCommand? command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.guilds";

        await using NpgsqlDataReader? reader = await ExecuteReader(command);
        List<GuildsRow> guilds = [];
        while (await reader.ReadAsync()) guilds.Add(new GuildsRow(ConnectionString, HandlersGroup, reader));

        return guilds;
    }
}