using Bot.Database.Handlers;
using Bot.Database.Handlers.Config;
using Bot.Database.Handlers.Filter;
using Bot.Database.Handlers.Public;
using Bot.Database.Handlers.Reactions;
using Handler = Bot.Database.Handlers.Filter.Handler;

namespace Bot.Database;

public class Database
{
    /// <summary>
    ///     Database Handlers.
    /// </summary>
    public readonly HandlersGroup Handlers;


    public Database
    (
        string host,
        int port,
        string username,
        string database,
        string password
    )
    {
        string connectionString = $"Host={host};Port={port};Username={username};Database={database};Password={password};Include Error Detail=true;";

        // Create handlers
        Handlers = new HandlersGroup
        {
            Config = new Handlers.Config.Handler(connectionString),
            Filter = new Handler(connectionString),
            Public = new Handlers.Public.Handler(connectionString),
            Reactions = new Handlers.Reactions.Handler(connectionString)
        };

        // Now give all handlers access to each other 
        foreach (BaseHandler handler in Handlers.Handlers) handler.Populate(Handlers);
    }
}