using Bot.Database.Types.Public;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public;

public class PublicGuildsHandler(string connectionString) : BaseHandler(connectionString)
{
    /// <summary>
    ///     Get a guild from the database.
    /// </summary>
    /// <param name="id">Guild ID</param>
    /// <returns>Guild.</returns>
    public async Task<PublicGuild?> Get(ulong id)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.guilds WHERE id = @id";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new PublicGuild(ConnectionString, HandlersGroup, reader);
    }

    public async Task<PublicGuild> Get(ulong id, string username)
    {
        // Check if the guild already exists
        PublicGuild? guild = await Get(id);
        if (guild != null) return guild;

        // Create a new guild
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO public.guilds (id, name) VALUES (@id, @name)";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = username });

        await ExecuteNonQuery(command);
        return await Get(id)! ?? throw new MissingMemberException();
    }

    public async Task<PublicGuild> Get(DiscordGuild guild)
    {
        return await Get(guild.Id, guild.Name);
    }
}