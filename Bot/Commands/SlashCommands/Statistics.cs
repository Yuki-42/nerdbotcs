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

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class StatisticsCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("statistics", "Statistics commands.")]
    public class StatisticsCommandGroup : ApplicationCommandsModule
    {
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
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                PublicUser user = await handler.Users.Get(ctx.User);

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