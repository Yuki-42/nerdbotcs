using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Bot.Database.Types.Public.Views;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class GlobalMessageViewRow(string connectionString, HandlersGroup handlersGroup, IDataRecord reader) : BaseViewRow(connectionString, handlersGroup)
{
    /// <summary>
    ///     User id.
    /// </summary>
    public ulong UserId { get; } = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));
    
    /// <summary>
    ///     The user.
    /// </summary>
    public UsersRow User => HandlersGroup.Public.Users.Get(UserId).Result!;
    
    /// <summary>
    ///     The number of messages sent by the user in the channel.
    /// </summary>
    public long MessagesSent { get; } = reader.GetInt64(reader.GetOrdinal("messages_sent"));
}