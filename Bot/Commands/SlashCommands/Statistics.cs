using System.Diagnostics.CodeAnalysis;
using Bot.Commands.Logic;
using Bot.Database.Handlers.Public;
using Bot.Database.Types.Public;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.SlashCommands;

public enum LeaderboardContext
{
    Global = 0,
    Guild = 1,
    Channel = 2
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class StatisticsCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("statistics", "Statistics commands.")]
    public class StatisticsCommandGroup : ApplicationCommandsModule
    {
        [SlashCommand("leaderboard", "Shows the leaderboard.")]
        public async Task LeaderboardCommand(
            InteractionContext ctx,
            [Option("context", "The context to show the leaderboard for.")]
            LeaderboardContext context = LeaderboardContext.Guild
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    Content = "Constructing leaderboard..."
                });
            
            // Check what context to use for the leaderboard
            switch (context)
            {
                case LeaderboardContext.Global:
                    // Permissions check
                    int permission = await Shared.CheckPermissions(ctx);
                    if (permission != 1)
                    {
                        await ctx.EditResponseAsync(
                            new DiscordWebhookBuilder
                            {
                                Content = "You do not have permission to run this command."
                            });
                        return;
                    }
                    
                    // Get the public handler
                    Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                    
                    
                    break;
                case LeaderboardContext.Guild:
                    break;
                case LeaderboardContext.Channel:
                    break;
                default:
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder
                        {
                            Content = "This command is not yet implemented."
                        });
                    break;
            }
        }

        [SlashCommand("individual", "Shows the statistics for an individual.")]
        public async Task IndividualCommand(
            InteractionContext ctx,
            [Option("user", "The user to show statistics for.")]
            DiscordUser? user = null,
            [Option("context", "The context to show the statistics for.")]
            LeaderboardContext context = LeaderboardContext.Global
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    Content = "This command is not yet implemented."
                });
        }


        /// <summary>
        ///     Audit related commands.
        /// </summary>
        [SlashCommandGroup("audit", "Audit commands.")]
        public class AuditGroup : ApplicationCommandsModule
        {
            /// <summary>
            ///     Audits all categories.
            /// </summary>
            /// <param name="ctx">Context</param>
            [SlashCommand("all", "Audits all categories.")]
            public async Task AuditAllCommand(InteractionContext ctx)
            {
                // Check if the user is a global bot admin
                Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                UsersRow user = await handler.Users.Get(ctx.User);

                // Perform permissions checks
                int permission = await Shared.CheckPermissions(ctx);

                switch (permission)
                {
                    case 0:
                        await ctx.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder
                            {
                                Content = "You do not have permission to run this command."
                            });
                        return;
                    case 1:
                        await Statistics.AuditAllGlobalAdmin(ctx);
                        return;
                    case 2:
                        await Statistics.AuditAllServerAdmin(ctx);
                        return;
                }
            }

            /// <summary>
            ///     Audits user data.
            /// </summary>
            /// <param name="ctx">Context</param>
            [SlashCommand("users", "Audits global user data.")]
            public async Task AuditUsersCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Running audit, this may take a while."
                    });

                // Do permissions checks
                int permission = await Shared.CheckPermissions(ctx);

                switch (permission)
                {
                    case 0:
                        await ctx.EditResponseAsync(
                            new DiscordWebhookBuilder
                            {
                                Content = "You do not have permission to run this command."
                            });
                        return;
                    case 1:
                        await Statistics.AuditAllUsers(ctx);
                        break;
                    case 2:
                        await Statistics.AuditGuildUsers(ctx, ctx.Guild);
                        break;
                }

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Audit completed."
                    });
            }

            /// <summary>
            ///     Audits guild data.
            /// </summary>
            /// <param name="ctx">Context</param>
            [SlashCommand("guilds", "Audits stored server data.")]
            public async Task AuditGuildsCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Running audit, this may take a while."
                    });

                // Do permissions checks
                int permission = await Shared.CheckPermissions(ctx);

                switch (permission)
                {
                    case 0:
                        await ctx.EditResponseAsync(
                            new DiscordWebhookBuilder
                            {
                                Content = "You do not have permission to run this command."
                            });
                        return;
                    case 1:
                        await Statistics.AuditAllGuilds(ctx);
                        break;
                    case 2:
                        await Statistics.AuditGuild(ctx, ctx.Guild);
                        break;
                }

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Audit completed."
                    });
            }

            /// <summary>
            ///     Audits message data.
            /// </summary>
            /// <param name="ctx">Context</param>
            [SlashCommand("messages", "Audits message statistics.")]
            public async Task AuditMessagesCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Running audit, this may take a while."
                    });

                // Do permissions checks
                int permission = await Shared.CheckPermissions(ctx);

                switch (permission)
                {
                    case 0:
                        await ctx.EditResponseAsync(
                            new DiscordWebhookBuilder
                            {
                                Content = "You do not have permission to run this command."
                            });
                        return;
                    case 1:
                        await Statistics.AuditAllMessages(ctx);
                        break;
                    case 2:
                        await Statistics.AuditGuildMessages(ctx, ctx.Guild);
                        break;
                }

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Audit completed."
                    });
            }
        }
    }
}