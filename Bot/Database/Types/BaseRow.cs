using System.Data;

namespace Bot.Database.Types;

/// <summary>
///     Base class for all database types.
/// </summary>
public class BaseRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader)
    : TypeBase(connectionString, handlersGroup)
{
    /// <summary>
    ///     Row id.
    /// </summary>
    public Guid Id { get; } = TryGetGuid(reader, "id") ?? Guid.Empty;

    /// <summary>
    ///     Row added to db.
    /// </summary>
    public DateTime CreatedAt { get; } = reader.GetDateTime(reader.GetOrdinal("created_at"));


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
}