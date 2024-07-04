using Bot.Database.Types.Public;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public;

public class PublicChannelsUsersHandler(string connectionString) : BaseHandler(connectionString)
{
    public async Task<PublicChannelUser?> Get(ulong id)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.channels_users WHERE id = @id;";
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new PublicChannelUser(ConnectionString, HandlersGroup, reader);
    }

    private async Task<PublicChannelUser?> PGet(ulong userId, ulong channelId)
    {
        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM public.channels_users WHERE user_id = @uid AND channel_id = @cid;";
        command.Parameters.Add(new NpgsqlParameter("uid", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("cid", NpgsqlDbType.Numeric) { Value = (long)channelId });

        await using NpgsqlDataReader reader = await ExecuteReader(command);
        return !reader.Read() ? null : new PublicChannelUser(ConnectionString, HandlersGroup, reader);
    }

    public async Task<PublicChannelUser> Get(ulong userId, ulong channelId)
    {
        // Check if the user already exists.
        PublicChannelUser? user = await PGet(userId, channelId);
        if (user != null) return user;

        // Get a new connection
        await using NpgsqlConnection connection = await Connection();
        await using NpgsqlCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO public.channels_users (user_id, channel_id) VALUES (@uid, @cid);";
        command.Parameters.Add(new NpgsqlParameter("uid", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("cid", NpgsqlDbType.Numeric) { Value = (long)channelId });

        await ExecuteNonQuery(command);

        return await PGet(userId, channelId) ?? throw new MissingMemberException();
    }

    public async Task<PublicChannelUser> Get(DiscordUser user, DiscordChannel channel)
    {
        return await Get(user.Id, channel.Id);
    }
}