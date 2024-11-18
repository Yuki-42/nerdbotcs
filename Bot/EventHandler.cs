using Bot.Database.Types;
using Bot.Database.Types.Reactions;
using DisCatSharp;
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
        var handler = database.Handlers.Public;

        // Get the user
        var discordUser = eventArgs.Author;
        var localUser = await handler.Users.Get(discordUser);

        // Get the channel and channel user
        var discordChannel = eventArgs.Channel;
        var localChannel = await handler.Channels.Get(discordChannel);
        var localChannelUser = await handler.ChannelUsers.Get(discordUser, discordChannel);

        // Get the guild and guild user
        var discordGuild = discordChannel.Guild;
        var localGuild = await handler.Guilds.Get(discordGuild);
        var localGuildUser = await handler.GuildUsers.Get(discordUser, discordGuild);


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
        var handler = database.Handlers.Public;
        var reactionsHandler = database.Handlers.Reactions;

        // Get the user
        var publicUser = await handler.Users.Get(eventArgs.Author);

        // Get the reactions for the user 
        IEnumerable<ReactionsRow> reactions = await reactionsHandler.GetReactions(publicUser.Id);

        // Try add the reaction to the message
        foreach (var reaction in reactions)
        {
            var success = reaction.TryGetEmoji(client, out var emoji);

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
        var handler = Database.Handlers.Config;

        // Get the bot status and status type
        var lStatus = await handler.Get("status");
        var lStatusType = await handler.Get("status_type");

        // Check if either values are null
        if (lStatus == null || lStatusType == null)
        {
            Console.WriteLine("Status or status type is null.");
            return;
        }

        // Check that the status type is an int
        if (!int.TryParse(lStatusType.Value, out var statusTypeInt))
        {
            Console.WriteLine("Could not parse status type to int.");
            return;
        }

        // Get the status type
        var activity = StatusType.GetActivityType(statusTypeInt, lStatus.Value!);

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