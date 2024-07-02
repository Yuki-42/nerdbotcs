﻿using System.Data;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace Bot.Database.Types.Public;

public class PublicGuildUser(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseType(connectionString, handlersGroup, reader)
{
    public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));
    public ulong GuildId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));

    public bool MessageTracking
    {
        get
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT message_tracking FROM public.guilds_users WHERE user_id = @user_id AND guild_id = @guild_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)GuildId });

            return (bool)command.ExecuteScalar()!;
        }
        set
        {
            using NpgsqlConnection connection = GetConnection();
            using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE public.guilds_users SET message_tracking = @value WHERE user_id = @user_id AND guild_id = @guild_id;";
            command.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlDbType.Numeric) { Value = (long)UserId });
            command.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlDbType.Numeric) { Value = (long)GuildId });

            command.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Boolean) { Value = value });
            ExecuteNonQuery(command);
        }
    }

    public async Task<PublicUser?> GetUser()
    {
        return await HandlersGroup.Public.Users.Get(UserId) ?? throw new MissingMemberException();
    }

    public async Task<PublicGuild?> GetGuild()
    {
        return await HandlersGroup.Public.Guilds.Get(GuildId) ?? throw new MissingMemberException();
    }
}