using Bot.Database.Types.Public;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public;

public class PublicGuildsUsersHandler(string connectionString) : BaseHandler(connectionString)
{
    public async Task<PublicGuildUser?> Get(ulong id)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.guilds_users WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new PublicGuildUser(ConnectionString, HandlersGroup, reader);
    }

    private async Task<PublicGuildUser?> PGet(ulong userId, ulong guildId)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.guilds_users WHERE user_id = @uid AND guild_id = @gid;";
        command.Parameters.Add(new NpgsqlParameter("uid", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("gid", NpgsqlDbType.Numeric) { Value = (long)guildId });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new PublicGuildUser(ConnectionString, HandlersGroup, reader);
    }

    public async Task<PublicGuildUser> Get(ulong userId, ulong guildId)
    {
        // Check if the user already exists.
        PublicGuildUser? user = await PGet(userId, guildId);
        if (user != null) return user;

        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO public.guilds_users (user_id, guild_id) VALUES (@uid, @gid);";
        command.Parameters.Add(new NpgsqlParameter("uid", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("gid", NpgsqlDbType.Numeric) { Value = (long)guildId });

        await ExecuteNonQuery(command);

        return await Get(userId, guildId) ?? throw new MissingMemberException();
    }

    public async Task<PublicGuildUser> Get(DiscordUser user, DiscordGuild guild)
    {
        return await Get(user.Id, guild.Id);
    }
}