using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Npgsql;

namespace Bot.Database.Types;

/// <summary>
///     Base class for all database types.
/// </summary>
public class BaseType(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
{
    public string ConnectionString { get; } = connectionString;
    
    protected NpgsqlConnection GetConnection()
    {
        NpgsqlConnection connection;
        int timeWaited = 0;
        while (true)
        {
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
                continue;
            }
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
        {
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
                continue;
            }
        }
        
        connection.Open();
        return connection;
    }

    /// <summary>
    ///     Handlers group.
    /// </summary>
    protected HandlersGroup HandlersGroup = handlersGroup;

    /// <summary>
    ///     Row id.
    /// </summary>
    public Guid Id { get; } = TryGetGuid(reader, "id") ?? Guid.Empty;

    /// <summary>
    ///     Row added to db.
    /// </summary>
    public DateTime CreatedAt { get; } = reader.GetDateTime(reader.GetOrdinal("created_at"));

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

    private static Guid? TryGetGuid(IDataRecord reader, string column)
    {
        try
        {
            return reader.GetGuid(reader.GetOrdinal("id"));
        }
        catch (InvalidCastException)
        {
            return null;
        }
    }
    
    protected static ulong? GetNullableUlong(IDataRecord reader, string column)
    {
        int ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : (ulong)reader.GetInt64(ordinal);
    }
}