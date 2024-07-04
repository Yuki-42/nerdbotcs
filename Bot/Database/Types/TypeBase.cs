﻿using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Npgsql;

namespace Bot.Database.Types;

public class TypeBase(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
{
    /// <summary>
    ///     Handlers group.
    /// </summary>
    protected HandlersGroup HandlersGroup = handlersGroup;

    /// <summary>
    ///     Connection string.
    /// </summary>
    private string ConnectionString { get; } = connectionString;
    
    protected NpgsqlConnection GetConnection()
    {
        NpgsqlConnection connection;
        int timeWaited = 0;
        while (true)
            try
            {
                connection = new NpgsqlConnection(ConnectionString);
                break;
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState != "53300") throw;
                Console.WriteLine($"Connection limit hit. Waiting 500ms before trying again. Total time waited {timeWaited}");
                Thread.Sleep(500);
                timeWaited += 500;
            }

        connection.Open();
        return connection;
    }

    /// <summary>
    ///     Database connection.
    /// </summary>
    protected async Task<NpgsqlConnection> GetConnectionAsync()
    {
        NpgsqlConnection connection;
        int timeWaited = 0;
        while (true)
            try
            {
                connection = new NpgsqlConnection(ConnectionString);
                break;
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState != "53300") throw;
                Console.WriteLine($"Connection limit hit. Waiting 500ms before trying again. Total time waited {timeWaited}");
                await Task.Delay(500);
                timeWaited += 500;
            }

        connection.Open();
        return connection;
    }

    /// <summary>
    ///     Execute a non query command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    public void ExecuteNonQuery(DbCommand command)
    {
        Debug.Assert(command.Connection is not null, "command.Connection != null");
        using DbTransaction transaction = command.Connection.BeginTransaction();
        command.Transaction = transaction;

        try
        {
            command.ExecuteNonQuery();
        }
        catch (Exception error)
        {
            transaction.Rollback();
            Console.WriteLine($"Failed to execute command: {error.Message}");
        }

        transaction.Commit();
    }
    
    /// <summary>
    /// Gets a nullable ulong from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="column">Column name.</param>
    /// <returns>Requested value if present, else null.</returns>
    protected static ulong? GetNullableUlong(IDataRecord reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : (ulong)reader.GetInt64(ordinal);
    }
}