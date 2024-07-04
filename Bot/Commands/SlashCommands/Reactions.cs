using Bot.Commands.Logic;
using Bot.Database.Handlers.Public;
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
    public class ReactionsGroup : ApplicationCommandsModule
    {
        [SlashCommand("list", "List all automatic reactions.")]
        public async Task ListReactionsCommand(
            InteractionContext ctx,
            [Option("user", "Target user.")] DiscordUser? user = null
        )
        {
            // Create response 
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "Listing reactions...", IsEphemeral = true });

            // Get the required services
            Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

            // Get the required handlers
            PublicHandler publicHandler = database.Handlers.Public;
            ReactionsHandler reactionsHandler = database.Handlers.Reactions;

            // Set target user if null
            user ??= ctx.User;

            // Get the user
            PublicUser publicUser = await publicHandler.Users.Get(user);

            // Check if the user has permission to list reactions
            if (!await Reactions.ListPermissionsCheck(ctx, user))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You do not have permission to list reactions for other users."));
                return;
            }

            // Get the reactions
            IEnumerable<ReactionsReaction> reactions = await reactionsHandler.GetReactions(user.Id);

            // Create the reactions text 
            string reactionsText = "";
            IEnumerable<ReactionsReaction> reactionsReactions = reactions.ToList();
            foreach (ReactionsReaction reaction in reactionsReactions)
            {
                string emoji;
                if (reaction.Emoji is null)
                {
                    reaction.TryGetEmoji(ctx.Client, out DiscordEmoji? dEmoji);
                    emoji = dEmoji?.Name ?? throw new InvalidOperationException("Both reaction.Emoji and discord emoji are null.");
                }
                else
                {
                    emoji = reaction.Emoji;
                }

                reactionsText +=
                    $"ID: `{reaction.Id}` | {emoji}{(reaction.ChannelId != null ? $" | Channel: {reaction.ChannelId.Value}" : "")}{(reaction.GuildId != null ? $" | Guild: {reaction.GuildId}" : "")}\n";
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = $"Adding reaction {emoji} to {user.Username}" });

            // Get the required services
            Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

            // Get the required handlers
            PublicHandler publicHandler = database.Handlers.Public;
            ReactionsHandler reactionsHandler = database.Handlers.Reactions;

            // Get the user
            PublicUser publicUser = await publicHandler.Users.Get(user);

            // For now, only allow the bot owner to use this command
            if (!await Reactions.AddPermissionsChecks(ctx, user))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You do not have permission to add reactions for other users."));
                return;
            }

            // Check if the emoji is valid
            if (!Reactions.CheckValidEmoji(ctx.Client, emoji))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid emoji."));
                return;
            }

            // Get the channel if it's not null
            PublicChannel? publicChannel = channel is null ? null : await publicHandler.Channels.Get(channel);

            // Get the guild if it's not null
            PublicGuild? publicGuild = channel is null ? null : await publicHandler.Guilds.Get(channel.Guild);

            // Add the reaction

            // Check if the reaction already exists
            if (await reactionsHandler.Exists(emoji, publicUser.Id, publicGuild?.Id, publicChannel?.Id))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Reaction {emoji} already exists for {user.Username} with the same settings."));
                return;
            }

            ReactionsReaction reaction = await reactionsHandler.New(emoji, publicUser, publicGuild, publicChannel);

            // Edit the response
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added reaction {emoji} to {user.Username}"));
        }

        [SlashCommand("remove", "Remove a reaction.")]
        public async Task RemoveReactionCommand(
            InteractionContext ctx,
            [Option("reaction-id", "ID of the reaction to remove.")]
            string reactionId,
            [Option("user", "User to remove reaction from.")]
            DiscordUser? user = null
        )
        {
            // Create response
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder { Content = "Removing reaction..." });

            // Get the required services
            Database.Database database = ctx.Services.GetRequiredService<Database.Database>();

            // Get the required handlers
            PublicHandler publicHandler = database.Handlers.Public;
            ReactionsHandler reactionsHandler = database.Handlers.Reactions;

            // Set target user if null
            user ??= ctx.User;

            // Get the user
            PublicUser publicUser = await publicHandler.Users.Get(user);

            // Check if the user has permission to remove reactions
            if (!await Reactions.RemovePermissionsCheck(ctx, user))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You do not have permission to remove reactions for other users."));
                return;
            }

            // Get the reaction
            ReactionsReaction? reaction = await reactionsHandler.Get(new Guid(reactionId));

            // Check if the reaction exists
            if (reaction is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Reaction not found."));
                return;
            }

            // Check if the user has permission to remove the reaction
            if (reaction.UserId != publicUser.Id)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You do not have permission to remove this reaction."));
                return;
            }

            // Remove the reaction
            await reaction.Delete();

            // Edit the response
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Removed reaction."));
        }
    }
}