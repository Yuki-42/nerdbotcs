using Bot.Commands.Logic;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Bot.Commands.SlashCommands;

/*
 * | Emoji Type | Is Valid |
 * | ---------- | -------- |
 * | Unicode    | %s      |
 * | Discord    | %s      |
 * | Guild      | %s      |
 */
public class Testing : ApplicationCommandsModule
{
	[SlashCommand("channel", "Gets channel info.")]
	public static async Task GetChannelCommand(
		InteractionContext ctx,
		[Option("channel", "The channel to get info from.")]
		DiscordChannel channel
	)
	{
		// Get channel from id
		await ctx.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder
			{
				Content = $"Channel: {channel.Name}\nGuild: {channel.Guild.Name}"
			});
	}

	[SlashCommand("valid-emoji", "Checks if an emoji is valid according to the bot. Used to triger a breakpoint")]
	public static async Task CheckValidEmojiCommand(
		InteractionContext ctx,
		[Option("emoji", "Emoji to check")] string emoji
	)
	{
		await ctx.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder
			{
				Content = $"Testing emoji `{emoji}`\n" +
				          "```\n| Emoji Type | Is Valid |\n" +
				          "| ---------- | -------- |\n" +
				          $"| Any        | {Reactions.CheckValidEmoji(ctx.Client, emoji)}    |\n" +
				          $"| Unicode    | {Reactions.CheckValidUnicodeEmoji(ctx.Client, emoji)}    |\n" +
				          $"| Discord    | {Reactions.CheckValidDiscordEmoji(ctx.Client, emoji)}    |\n" +
				          $"| Guild      | {Reactions.CheckValidGuildEmoji(ctx.Client, emoji)}    |```\n"
			}
		);
	}
}