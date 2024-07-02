using Bot.Database.Handlers;
using Bot.Database.Handlers.Config;
using Bot.Database.Handlers.Filter;
using Bot.Database.Handlers.Public;
using Bot.Database.Handlers.Reactions;

namespace Bot.Database;

public class HandlersGroup
{
    public required ConfigHandler Config;
    public required FilterHandler Filter;
    public required PublicHandler Public;
    public required ReactionsHandler Reactions;

    public BaseHandler[] Handlers =>
    [
        Config,
        Filter,
        Public,
        Reactions
    ];
}