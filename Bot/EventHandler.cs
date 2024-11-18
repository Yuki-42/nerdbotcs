using Bot.Database.Handlers.Public;
using Bot.Database.Types;
using Bot.Database.Types.Config;
using Bot.Database.Types.Public;
using Bot.Database.Types.Reactions;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using Microsoft.Extensions.Logging;

namespace Bot;

internal static class EventLogic
{
    public static async Task MessageStatisticsTask(DiscordClient client, MessageCreateEventArgs eventArgs,
        Database.Database database)
    {
        // Get the required services
        Handler? handler = database.Handlers.Public;

        // Get the user
        DiscordUser? discordUser = eventArgs.Author;
        UsersRow? localUser = await handler.Users.Get(discordUser);

        // Get the channel and channel user
        DiscordChannel? discordChannel = eventArgs.Channel;
        ChannelsRow? localChannel = await handler.Channels.Get(discordChannel);
        ChannelsUsersRow? localChannelUser = await handler.ChannelUsers.Get(discordUser, discordChannel);

        // Get the guild and guild user
        DiscordGuild? discordGuild = discordChannel.Guild;
        GuildsRow? localGuild = await handler.Guilds.Get(discordGuild);
        GuildsUsersRow? localGuildUser = await handler.GuildUsers.Get(discordUser, discordGuild);


        // Check if statistics tracking is enabled
        if (!localUser.MessageTracking) return;
        if (!localChannel.MessageTracking) return;
        if (!localChannelUser.MessageTracking) return;
        if (!localGuild.MessageTracking) return;
        if (!localGuildUser.MessageTracking) return;

        // Update the statistics
        localChannelUser.MessagesSent++;
    }

    public static async Task ReactionsTask(DiscordClient client, MessageCreateEventArgs eventArgs,
        Database.Database database)
    {
        // Get the required services
        Handler? handler = database.Handlers.Public;
        Database.Handlers.Reactions.Handler? reactionsHandler = database.Handlers.Reactions;

        // Get the user
        UsersRow? publicUser = await handler.Users.Get(eventArgs.Author);

        // Get the reactions for the user 
        IEnumerable<ReactionsRow> reactions = await reactionsHandler.GetReactions(publicUser.Id);

        // Try add the reaction to the message
        foreach (ReactionsRow? reaction in reactions)
        {
            bool success = reaction.TryGetEmoji(client, out DiscordEmoji? emoji);

            if (!success)
            {
                await reaction.Delete();
                continue;
            }

            // Add the reaction
            await eventArgs.Message.CreateReactionAsync(emoji!);
        }
    }
}

[EventHandler]
public class EventHandler(Database.Database database)
{
    private Database.Database Database { get; } = database;

    // Create a method to handle exceptions in the event handler
    private static void HandleException(Exception exception)
    {
        Console.WriteLine(exception);
    }

    [Event(DiscordEvent.Ready)]
    private async Task OnReady(DiscordClient client, ReadyEventArgs eventArgs)
    {
        client.Logger.Log(LogLevel.Information, "Bot is ready to process events.");
        Console.WriteLine("Bot is ready to process events.");

        // Get the config service
        Database.Handlers.Config.Handler? handler = Database.Handlers.Config;

        // Get the bot status and status type
        ConfigRow? lStatus = await handler.Get("status");
        ConfigRow? lStatusType = await handler.Get("status_type");

        // Check if either values are null
        if (lStatus == null || lStatusType == null)
        {
            Console.WriteLine("Status or status type is null.");
            return;
        }

        // Check that the status type is an int
        if (!int.TryParse(lStatusType.Value, out int statusTypeInt))
        {
            Console.WriteLine("Could not parse status type to int.");
            return;
        }

        // Get the status type
        DiscordActivity? activity = StatusType.GetActivityType(statusTypeInt, lStatus.Value!);

        // Set the bot status
        await client.UpdateStatusAsync(activity);
    }

    [Event(DiscordEvent.MessageCreated)]
    private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs eventArgs)
    {
        // First do statistics counting
        await EventLogic.MessageStatisticsTask(client, eventArgs, Database);

        _ = Task.Run(async () =>
        {
            try
            {
                await EventLogic.ReactionsTask(client, eventArgs, Database);
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        });
    }
}