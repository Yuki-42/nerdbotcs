using Bot.Database.Handlers.Public;
using Bot.Database.Types.Public;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.Logic;

public class Shared
{
	/// <summary>
	///  Creates a simple permissions integer.
	/// </summary>
	/// <param name="ctx">Context</param>
	/// <returns>0: No administrator, 1: Global bot admin, 2: Server admin</returns>
	public static async Task<int> CheckPermissions(BaseContext ctx)
	{
		// Check if the user is a global bot admin
		PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
		UsersRow user = await handler.Users.Get(ctx.User);

		// Check if the user is a global bot admin
		if (user.Admin) return 1;

		// Check if the user is a server admin
		if (ctx.GuildId != null && ctx.Member!.Permissions.HasPermission(Permissions.Administrator)) return 2;

		// If the user is not a global bot admin or a server admin, return an error
		return 0;
	}
}