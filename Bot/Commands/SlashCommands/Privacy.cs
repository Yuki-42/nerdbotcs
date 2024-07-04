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
public class PrivacyCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("privacy", "Privacy settings.")]
    public class PrivacyCommandsGroup : ApplicationCommandsModule
    {
        [SlashCommand("opt-out", "Quickly opt out of all user data collection.")]
        public async Task QuickOutCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    Content = "Opting out of all user data collection..."
                });

            // Get the user
            PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

            PublicUser user = await handler.Users.Get(ctx.User);

            user.MessageTracking = false;

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder
                {
                    Content =
                        "Opt out completed. Your data will no longer be tracked by the bot.\nShould you wish to re-enable data tracking at any time, use the `/statistics privacy message-tracking opt-in` command."
                });
        }

        [SlashCommand("opt-in", "Quickly opt in to user data collection. Note: This will not overwrite any more-specific settings.")]
        public async Task QuickInCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    Content = "Opting in to user data collection..."
                });

            // Get the user
            PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

            PublicUser user = await handler.Users.Get(ctx.User);

            user.MessageTracking = true;

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder
                {
                    Content = "Opt in completed. Your data will now be tracked by the bot. \n" +
                              "Should you wish to disable data tracking at any time, use the `/statistics privacy message-tracking opt-out` command.\n" +
                              "Please note that this will not overwrite any more specific tracking rules, so if you have disabled tracking for a specific channel or server, that setting will still apply."
                });
        }


        [SlashCommandGroup("admin", "Manage privacy settings for the server.")]
        public class AdminGroup : ApplicationCommandsModule
        {
            [SlashCommand("toggle-server", "Toggle tracking for the server.")]
            public static async Task ServerToggleCommand(
                InteractionContext ctx,
                [Option("value", "Value to set")] bool? value = null
            )
            {
                await Privacy.ToggleServerMessageCollection(ctx, ctx.Guild, value);
            }

            [SlashCommand("toggle-channel", "Toggle tracking for the channel.")]
            public static async Task ChannelToggleCommand(
                InteractionContext ctx,
                [Option("channel", "Channel to toggle collection for. Defaults to the current channel.")]
                DiscordChannel? channel = null,
                [Option("value", "Value to set")] bool? value = null
            )
            {
                await Privacy.ToggleChannelMessageCollection(ctx, channel, value);
            }
        }


        [SlashCommandGroup("out", "Opt out of message tracking.")]
        public class MessageTrackingOutGroup : ApplicationCommandsModule
        {
            [SlashCommand("global", "Opt out of global message tracking.")]
            public async Task GlobalOutCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting out of global message tracking..."
                    });

                // Get the user
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                PublicUser user = await handler.Users.Get(ctx.User);

                user.MessageTracking = false;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Opt out completed. Your messages will no longer be counted by the bot."
                    });
            }

            [SlashCommand("server", "Opt out of server message tracking.")]
            public async Task ServerOutCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting out of server message tracking..."
                    });

                // Check if the command was run in a DM with no server specified
                if (ctx.Guild is null)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder
                        {
                            Content = "You must run this command in a server to opt out of server message tracking."
                        });
                    return;
                }

                // Get the GuildUser
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                DiscordGuild server = ctx.Guild;

                // Check if server is still null
                if (server is null) throw new InvalidOperationException();

                PublicGuildUser user = await handler.GuildUsers.Get(ctx.User, server);
                user.MessageTracking = false;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Opt out completed. Your messages will no longer be counted by the bot in this server."
                    });
            }

            [SlashCommand("channel", "Opt out of channel message tracking.")]
            public async Task ChannelOutCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting out of channel message tracking..."
                    });

                // Get the GuildUser
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                DiscordChannel server = ctx.Channel;

                // Check if server is still null
                if (server is null) throw new InvalidOperationException();

                PublicChannelUser user = await handler.ChannelUsers.Get(ctx.User, server);
                user.MessageTracking = false;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = $"Opt out completed. Your messages will no longer be counted by the bot in {ctx.Channel.Mention}."
                    });
            }
        }

        [SlashCommandGroup("in", "Opt in to message tracking.")]
        public class MessageTrackingInGroup : ApplicationCommandsModule
        {
            [SlashCommand("global", "Opt in to global message tracking.")]
            public async Task GlobalInCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting in to global message tracking..."
                    });

                // Get the user
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                PublicUser user = await handler.Users.Get(ctx.User);

                user.MessageTracking = true;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Opt in completed. Your messages will now be counted by the bot.\n" +
                                  "Please note that this will not overwrite any more specific tracking rules, so if you have disabled tracking for a specific channel or server, that setting will still apply."
                    });
            }

            [SlashCommand("server", "Opt in to server message tracking.")]
            public async Task ServerInCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting in to server message tracking..."
                    });

                // Check if the command was run in a DM with no server specified
                if (ctx.Guild is null)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder
                        {
                            Content = "You must run this command in a server to opt in to server message tracking."
                        });
                    return;
                }

                // Get the GuildUser
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                DiscordGuild server = ctx.Guild;

                // Check if server is still null
                if (server is null) throw new InvalidOperationException();

                PublicGuildUser user = await handler.GuildUsers.Get(ctx.User, server);
                user.MessageTracking = true;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Opt in completed. Your messages will now be counted by the bot.\n" +
                                  "Please note that this will not overwrite any more specific tracking rules, so if you have disabled tracking for a specific channel, that setting will still apply."
                    });
            }

            [SlashCommand("channel", "Opt in to channel message tracking.")]
            public async Task ChannelInCommand(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder
                    {
                        Content = "Opting in to channel message tracking..."
                    });

                // Check if the command was run in a DM with no server specified
                if (ctx.Guild is null)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder
                        {
                            Content = "You must run this command in a server to opt in to channel message tracking."
                        });
                    return;
                }

                // Get the GuildUser
                PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
                DiscordGuild server = ctx.Guild;

                // Check if server is still null
                if (server is null) throw new InvalidOperationException();

                PublicGuildUser user = await handler.GuildUsers.Get(ctx.User, server);
                user.MessageTracking = true;

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "Opt in completed. Your messages will now be counted by the bot."
                    });
            }
        }
    }
}