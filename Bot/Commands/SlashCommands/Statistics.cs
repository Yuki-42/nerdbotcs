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
    public class StatisticsComandGroup : ApplicationCommandsModule
    {
        /// <summary>
        ///     Audit related commands.
        /// </summary>
        [SlashCommandGroup("audit", "Audit commands.")]
        public class AuditGroup : ApplicationCommandsModule
        {
            private static readonly string[] AuditCompletion =
            [
                "Running %s audit, this may take a while.\n- :red_square: Users\n- :red_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
                "Running %s audit, this may take a while.\n- :green_square: Users\n- :red_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
                "Running %s audit, this may take a while.\n- :green_square: Users\n- :green_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
                "Running %s audit, this may take a while.\n- :green_square: Users\n- :green_square: Servers\n- :green_square: Channels\n- :red_square: Messages",
                "%s audit completed.\n- :green_square: Users\n- :green_square: Guilds\n- :green_square: Channels\n- :green_square: Messages"
            ];

            private static async Task AuditAllGlobalAdmin(BaseContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = AuditCompletion[0].Replace("%s", "global")
                    });

                await Statistics.AuditAllUsers(ctx);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[1].Replace("%s", "global")
                    });

                await Statistics.AuditAllGuilds(ctx);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[2].Replace("%s", "global")
                    });

                await Statistics.AuditAllChannels(ctx);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[3].Replace("%s", "global")
                    });

                await Statistics.AuditAllMessages(ctx);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[4].Replace("%s", "Global")
                    });
            }

            private static async Task AuditAllServerAdmin(BaseContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = AuditCompletion[0].Replace("%s", "server")
                    });

                if (ctx.Guild is null) throw new InvalidOperationException();

                await Statistics.AuditGuildUsers(ctx, ctx.Guild);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[1].Replace("%s", "server")
                    });

                await Statistics.AuditGuild(ctx, ctx.Guild);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[2].Replace("%s", "server")
                    });

                await Statistics.AuditGuildChannels(ctx, ctx.Guild);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[3].Replace("%s", "server")
                    });

                await Statistics.AuditGuildMessages(ctx, ctx.Guild);

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = AuditCompletion[4].Replace("%s", "server")
                    });
            }

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
                        await AuditAllGlobalAdmin(ctx);
                        return;
                    case 2:
                        await AuditAllServerAdmin(ctx);
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