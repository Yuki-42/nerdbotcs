using System.Diagnostics;
using Bot.Database.Handlers.Public;
using Bot.Database.Types.Public;
using DisCatSharp;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.Logic;

public class Privacy
{
    /// <summary>
    ///     Checks if the channel is accessible by the bot.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public static async Task<bool> CheckChannelAccessible(BaseContext ctx, DiscordChannel channel)
    {
        // Get the client
        DiscordClient client = ctx.Client;

        // Check if the channel is accessible by the bot
        try
        {
            await client.GetChannelAsync(channel.Id);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return false;
        }
    }

    public static async Task<bool> CheckBotInGuild(BaseContext ctx, DiscordGuild? guild)
    {
        // Get the client
        DiscordClient client = ctx.Client;

        if (guild is null) return false;

        // Check if the bot is in the guild
        try
        {
            await client.GetGuildAsync(guild.Id);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return false;
        }
    }

    public static async Task ToggleServerMessageCollection(
        InteractionContext ctx,
        DiscordGuild? guild,
        bool? value = null
    )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = "Toggling user data collection for the server..."
            });

        if (guild is null && ctx.Guild is null)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder
                {
                    Content = "You must specify a server to toggle data collection for."
                });
            return;
        }

        if (!await CheckBotInGuild(ctx, guild))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder
                {
                    Content = "The bot is not in the specified server."
                });
            return;
        }

        // Check permissions
        int permission = await Shared.CheckPermissions(ctx);

        switch (permission)
        {
            case 0:
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "You do not have permission to execute this command."
                    });
                return;
            case 1:
                break;
            case 2:
                break;
        }

        // Get the guild
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;
        guild ??= ctx.Guild;

        Debug.Assert(guild is not null, nameof(guild) + " != null");

        GuildsRow lGuild = await handler.Guilds.Get(guild);
        lGuild.MessageTracking = value ?? !lGuild.MessageTracking;

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = $"Toggled message tracking for {guild.Name} to {lGuild.MessageTracking}."
            });
    }

    public static async Task ToggleChannelMessageCollection(
        InteractionContext ctx,
        DiscordChannel? channel,
        bool? value = null
    )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = "Toggling user data collection for the channel..."
            });

        channel ??= ctx.Channel; // Ensure that the channel is not null

        if (!await CheckChannelAccessible(ctx, channel))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder
                {
                    Content = "The bot cannot access the specified channel."
                });
            return;
        }

        // Check permissions
        int permission = await Shared.CheckPermissions(ctx);

        switch (permission)
        {
            case 0:
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder
                    {
                        Content = "You do not have permission to execute this command."
                    });
                return;
            case 1:
                break;
            case 2:
                break;
        }

        // Get the guild
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        ChannelsRow lChannel = await handler.Channels.Get(channel);
        lChannel.MessageTracking = value ?? !lChannel.MessageTracking;

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = $"Toggled message tracking for {channel.Mention} to `{lChannel.MessageTracking}`."
            });
    }
}