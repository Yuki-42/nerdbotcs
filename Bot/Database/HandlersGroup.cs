using Bot.Database.Handlers;
using Config = Bot.Database.Handlers.Config;
using Filter = Bot.Database.Handlers.Filter;
using Public = Bot.Database.Handlers.Public;
using Reactions = Bot.Database.Handlers.Reactions;


namespace Bot.Database;

public class HandlersGroup
{
	public required Config.Handler Config;
	public required Filter.Handler Filter;
	public required Public.Handler Public;
	public required Reactions.Handler Reactions;

	public IEnumerable<BaseHandler> Handlers =>
	[
		Config,
		Filter,
		Public,
		Reactions
	];
}