using Bot.Database.Types.Public;
using DisCatSharp.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Handlers.Public;

public class ChannelsHandler(string connectionString) : BaseHandler(connectionString)
{
	public async Task<ChannelsRow?> Get(ulong id)
	{
		// Get a new connection
		await using NpgsqlConnection? connection = await Connection();
		await using NpgsqlCommand? command = connection.CreateCommand();
		command.CommandText = "SELECT * FROM public.channels WHERE id = @id";
		command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

		await using NpgsqlDataReader? reader = await ExecuteReader(command);
		return !reader.Read() ? null : new ChannelsRow(ConnectionString, HandlersGroup, reader);
	}

    /// <summary>
    ///  Adds a new channel to the database.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="guildId"></param>
    /// <param name="name">Channel name</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<ChannelsRow> Get(ulong id, ulong? guildId, string? name = null)
	{
		// Check if the channel already exists.
		ChannelsRow? channel = await Get(id);
		if (channel != null) return channel;

		// The channel does not exist in the db, add it.
		if (name is null) throw new Exception("Channel does not exist in the database and no name was provided.");

		// Get a new connection
		await using NpgsqlConnection? connection = await Connection();
		await using NpgsqlCommand? command = connection.CreateCommand();

		if (guildId == null)
		{
			command.CommandText = "INSERT INTO public.channels (id, name) VALUES (@id, @name)";
		}
		else
		{
			command.CommandText = "INSERT INTO public.channels (id, guild_id, name) VALUES (@id, @guild_id, @name)";
			command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)guildId });
		}

		command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
		command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });

		await ExecuteNonQuery(command);

		return await Get(id) ?? throw new MissingMemberException();
	}

	public async Task<ChannelsRow> Get(DiscordChannel channel)
	{
		return await Get(channel.Id, channel.GuildId, channel.Name);
	}

	public async Task<IReadOnlyList<ChannelsRow>> GetAll()
	{
		// Get a new connection
		await using NpgsqlConnection? connection = await Connection();
		await using NpgsqlCommand? command = connection.CreateCommand();
		command.CommandText = "SELECT * FROM public.channels";

		await using NpgsqlDataReader? reader = await ExecuteReader(command);
		List<ChannelsRow> channels = [];
		while (await reader.ReadAsync()) channels.Add(new ChannelsRow(ConnectionString, HandlersGroup, reader));

		return channels;
	}

	public async Task<IReadOnlyList<ChannelsRow>> GetAll(ulong guildId)
	{
		// Get a new connection
		await using NpgsqlConnection? connection = await Connection();
		await using NpgsqlCommand? command = connection.CreateCommand();
		command.CommandText = "SELECT * FROM public.channels WHERE guild_id = @guild_id";
		command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)guildId });

		await using NpgsqlDataReader? reader = await ExecuteReader(command);
		List<ChannelsRow> channels = [];
		while (await reader.ReadAsync()) channels.Add(new ChannelsRow(ConnectionString, HandlersGroup, reader));

		return channels;
	}
}