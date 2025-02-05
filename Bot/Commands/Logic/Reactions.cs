using Bot.Database;
using Bot.Database.Handlers.Public;
using Bot.Database.Types.Public;
using DisCatSharp;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.Logic;

public class Reactions
{
    /// <summary>
    ///  Checks if an emoji is valid on discord.
    /// </summary>
    /// <remarks>
    ///  Works with unicode emojis, discord emojis, and guild emojis.
    /// </remarks>
    /// <code>
    /// Uses a combination of regular expressions and the Discord API to check if an emoji is valid.
    /// </code>
    /// <param name="client">Discord client.</param>
    /// <param name="emoji">Emoji text to check.</param>
    /// <returns>If the passed string is a valid discord emoji.</returns>
    public static bool CheckValidEmoji(DiscordClient client, string emoji)
	{
		return CheckValidUnicodeEmoji(client, emoji) || CheckValidDiscordEmoji(client, emoji) ||
		       CheckValidGuildEmoji(client, emoji);
	}

	private static bool CheckValidUnicodeEmoji(DiscordClient client, string emoji)
	{
		return RegularExpressions.UnicodeEmoji.Match(emoji).Success &&
		       DiscordEmoji.TryFromUnicode(client, emoji, out _);
	}

	private static bool CheckValidDiscordEmoji(DiscordClient client, string emoji)
	{
		return RegularExpressions.DiscordEmoji.Match(emoji).Success && DiscordEmoji.TryFromName(client, emoji, out _);
	}

	private static bool CheckValidGuildEmoji(DiscordClient client, string emoji)
	{
		// Check if the emoji has valid syntax before checking with the API
		if (!RegularExpressions.GuildEmoji.Match(emoji).Success) return false;

		// Extract the emoji ID and check with the API
		ulong emojiId = RegularExpressions.ExtractId(emoji);
		return DiscordEmoji.TryFromGuildEmote(client, emojiId, out _);
	}

    /// <summary>
    ///  Checks if a user has the appropriate permissions to list reactions.
    /// </summary>
    /// <param name="ctx">Context.</param>
    /// <param name="targetUser">The target user for the emoji list.</param>
    /// <returns>If the list action should proceed or not.</returns>
    public static async Task<bool> ListPermissionsCheck(BaseContext ctx, DiscordUser targetUser)
	{
		// Check if the user is the target user
		if (ctx.User.Id == targetUser.Id) return true;

		// Get the required handlers
		Handler? publicHandler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

		// Check if the user is a global bot admin
		UsersRow? publicUser = await publicHandler.Users.Get(ctx.User);
		return publicUser.Admin;
	}

    /// <summary>
    ///  Checks if an emoji add command has appropriate permissions.
    /// </summary>
    /// <param name="ctx">Context.</param>
    /// <param name="targetUser">Target user for emoji add.</param>
    /// <returns>If the reaction add should proceed or not.</returns>
    public static async Task<bool> AddPermissionsChecks(BaseContext ctx, DiscordUser targetUser)
	{
		// First check if the target user and invoker are the same
		if (ctx.User.Id == targetUser.Id) return true;

		// Get the required handlers
		Handler publicHandler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

		// Check if the user is a global bot admin
		UsersRow publicUser = await publicHandler.Users.Get(ctx.User);
		if (publicUser.Admin) return true;

		// If the guild is null then the user is not a server admin
		if (ctx.Guild is null) return false;

		// Check if the user is a server admin
		DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
		return member.Permissions.HasPermission(Permissions.ModerateMembers);
	}


	public static async Task<bool> RemovePermissionsChecks(BaseContext ctx, DiscordUser targetUser, DiscordGuild? targetGuild = null)
	{
		// Checks required:
		// 1. User is themselves
		// 2. User is a bot admin
		// 3. User is a guild admin and executing the command for the current guild

		// Check if the target and invoker are the same
		if (ctx.User.Id == targetUser.Id) return true;

		// Get required handlers
		Handler publicHandler =
	}
}