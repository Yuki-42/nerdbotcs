using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class UsersRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
	: BaseRow(connectionString, handlersGroup, reader)
{
	/// <summary>
	///  User's discord id.
	/// </summary>
	public new ulong Id { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("id"));

	/// <summary>
	///  User's in-database username.
	/// </summary>
	public string Username
	{
		get
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "SELECT username FROM public.users WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			return (command.ExecuteScalar() as string)!;
		}
		set
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "UPDATE public.users SET username = @value WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			command.Parameters.Add(new NpgsqlParameter("value", DbType.String) { Value = value });
			ExecuteNonQuery(command);
		}
	}

	/// <summary>
	///  If the user is banned from the bot.
	/// </summary>
	public bool Banned
	{
		get
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "SELECT banned FROM public.users WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			return (bool)command.ExecuteScalar()!;
		}
		set
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "UPDATE public.users SET banned = @value WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
			ExecuteNonQuery(command);
		}
	}

	public bool MessageTracking
	{
		get
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "SELECT message_tracking FROM public.users WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			return (bool)command.ExecuteScalar()!;
		}
		set
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "UPDATE public.users SET message_tracking = @value WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
			ExecuteNonQuery(command);
		}
	}

	public bool Admin
	{
		get
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "SELECT admin FROM public.users WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			return (bool)command.ExecuteScalar()!;
		}
		set
		{
			using NpgsqlConnection connection = GetConnection();
			using NpgsqlCommand command = connection.CreateCommand();
			command.CommandText = "UPDATE public.users SET admin = @value WHERE id = @id;";
			command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)Id });

			command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
			ExecuteNonQuery(command);
		}
	}

	public async Task Delete()
	{
		await using NpgsqlConnection connection = await GetConnectionAsync();
		await using NpgsqlCommand command = connection.CreateCommand();
		command.CommandText = "DELETE FROM public.users WHERE id = @id;";
		command.Parameters.Add(new NpgsqlParameter("id", DbType.VarNumeric) { Value = (long)Id });

		await command.ExecuteNonQueryAsync();
	}
}