using Bot.Commands.Logic;
using Bot.Database.Handlers.Reactions;
using Bot.Database.Types.Public;
using Bot.Database.Types.Reactions;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global
namespace Bot.Commands.SlashCommands;

public class ReactionsCommands : ApplicationCommandsModule
{
	[SlashCommandGroup("reactions", "Manage automatic reactions.")]
	// ReSharper disable once UnusedType.Global  // Rider is not very smart...
	public class ReactionsGroup : ApplicationCommandsModule
	{
		[SlashCommand("list", "List all automatic reactions.")]
		public async Task ListReactionsCommand(
			InteractionContext ctx,
			[Option("user", "Target user.")] DiscordUser? user = null
		)
		{
			// Create response
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder { Content = "Listing reactions...", IsEphemeral = true });

			// Get the required services
			Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

			// Get the required handlers
			ReactionsHandler reactionsHandler = database.Handlers.Reactions;

			// Set target user if null
			user ??= ctx.User;

			// Check if the user has permission to list reactions
			if (!await Reactions.ListPermissionsCheck(ctx, user))
			{
				await ctx.EditResponseAsync(
					new DiscordWebhookBuilder().WithContent(
						"You do not have permission to list reactions for other users."));
				return;
			}

			// Get the reactions
			IEnumerable<ReactionsRow> reactions = await reactionsHandler.GetReactions(user.Id);

			// Create the reactions text
			string reactionsText = "";
			IEnumerable<ReactionsRow> reactionsReactions = reactions.ToList();
			foreach (ReactionsRow? reaction in reactionsReactions)
			{
				string emoji;
				if (reaction.Emoji is null)
				{
					reaction.TryGetEmoji(ctx.Client, out DiscordEmoji? dEmoji);
					emoji = dEmoji?.Name ??
					        throw new InvalidOperationException("Both reaction.Emoji and discord emoji are null.");
				}
				else
				{
					emoji = reaction.Emoji;
				}

				reactionsText +=
					$"{emoji}{(reaction.ChannelId != null ? $" | Channel: {reaction.ChannelId.Value}" : "")}{(reaction.GuildId != null ? $" | Guild: {reaction.GuildId}" : "")}\n";
			}

			// Edit the response
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Reactions: \n" + reactionsText));
		}

		[SlashCommand("add", "Adds an automatic reaction.")]
		public async Task AddReactionCommand(
			InteractionContext ctx,
			[Option("user", "User to react to.")] DiscordUser user,
			[Option("emoji", "The emoji to react with.")]
			string emoji,
			[Option("channel", "Channel to react in.")]
			DiscordChannel? channel = null,
			[Option("server-only", "Limit reactions to this server.")]
			bool guild = false
		)
		{
			// Create response
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder { Content = $"Adding reaction {emoji} to {user.Username}" });

			// Get the required services
			Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

			// Get the required handlers
			Database.Handlers.Public.PublicHandler publicHandler = database.Handlers.Public;
			ReactionsHandler reactionsHandler = database.Handlers.Reactions;

			// Get the user
			UsersRow publicUser = await publicHandler.Users.Get(user);

			// For now, only allow the bot owner to use this command
			if (!await Reactions.AddPermissionsChecks(ctx, user))
			{
				await ctx.EditResponseAsync(
					new DiscordWebhookBuilder().WithContent(
						"You do not have permission to add reactions for other users."));
				return;
			}

			// Check if the emoji is valid
			if (!Reactions.CheckValidEmoji(ctx.Client, emoji))
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid emoji."));
				return;
			}

			// Get the channel if it is not null
			ChannelsRow? publicChannel = channel is null ? null : await publicHandler.Channels.Get(channel);

			// Get the guild if it is not null
			GuildsRow? publicGuild = channel is null ? null : await publicHandler.Guilds.Get(channel.Guild);

			// Get reactions that are applicable in this guild & channel for this user


			// Check if the reaction already exists
			if (await reactionsHandler.Exists(emoji, publicUser.Id, publicChannel?.Id, publicGuild?.Id))
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
					$"Reaction {emoji} already exists for {user.Username} with the same settings."));
				return;
			}

			// Add the reaction
			await reactionsHandler.New(emoji, publicUser, publicGuild, publicChannel);

			// Edit the response
			await ctx.EditResponseAsync(
				new DiscordWebhookBuilder().WithContent($"Added reaction {emoji} to {user.Username}"));
		}

		[SlashCommand("remove", "Remove a reaction.")]
		public async Task RemoveReactionCommand(
			InteractionContext ctx,
			[Option("reaction", "Reaction emoji")] string reactionStr,
			[Option("channel", "Channel the reaction is for.")]
			DiscordChannel? channel = null,
			[Option("guild-id", "Guild reaction occurs in. 0 for this guild.")]
			long? guildId = null, // This might break
			[Option("user", "User to remove reaction from.")]
			DiscordUser? user = null
		)
		{
			// Create response
			await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder { Content = "Removing reaction..." });

			// Check that the reaction is a valid reaction
			if (Reactions.CheckValidEmoji(ctx.Client, reactionStr))
			{
				await ctx.EditResponseAsync(
					new DiscordWebhookBuilder().WithContent($"{reactionStr} is not a valid discord reaction"));
				return;
			}

			// Get the required services
			Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

			// Get the required handlers
			ReactionsHandler reactionsHandler = database.Handlers.Reactions;

			// Set target user if null
			user ??= ctx.User;

			// Set target guild if 0
			DiscordGuild? targetGuild = null;
			if (guildId != null)
			{
				if (guildId != 0)
				{
					// Try and get the guild, if it is invalid throw an error
					if (!ctx.Client.TryGetGuild((ulong)guildId, out targetGuild))
					{
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid guild ID"));
						return;
					}
				}
				else
				{
					targetGuild = ctx.Guild;
				}
			}

			// Do permissions check
			if (!await Reactions.RemovePermissionsChecks(ctx, user, targetGuild))
			{
				await ctx.EditResponseAsync(
					new DiscordWebhookBuilder().WithContent("You do not have permissions to remove this reaction."));
				return;
			}

			// Get the reaction
			ReactionsRow? reaction = await reactionsHandler.Get(reactionStr, user.Id, ctx.ChannelId, ctx.GuildId);

			if (reaction is null)
			{
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Reaction does not exist."));
				return;
			}

			// Remove the reaction
			await reaction.Delete();

			// Edit the response
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Removed reaction."));
		}
	}
}