using System.Diagnostics;
using Bot.Database.Handlers.Public;
using Bot.Database.Handlers.Reactions;
using Bot.Database.Types;
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
    public static async Task MessageStatisticsTask(DiscordClient client, MessageCreateEventArgs eventArgs, Database.Database database)
    {
        // Get the required services
        PublicHandler handler = database.Handlers.Public;

        // Get the user
        DiscordUser discordUser = eventArgs.Author;
        PublicUser localUser = await handler.Users.Get(discordUser);

        // Get the channel and channel user
        DiscordChannel discordChannel = eventArgs.Channel;
        PublicChannel localChannel = await handler.Channels.Get(discordChannel);
        PublicChannelUser localChannelUser = await handler.ChannelUsers.Get(discordUser, discordChannel);

        // Get the guild and guild user
        DiscordGuild discordGuild = discordChannel.Guild;
        PublicGuild localGuild = await handler.Guilds.Get(discordGuild);
        PublicGuildUser localGuildUser = await handler.GuildUsers.Get(discordUser, discordGuild);



        // Check if statistics tracking is enabled
        if (!localUser.MessageTracking) { return; }
        if (!localChannel.MessageTracking) { return; }
        if (!localChannelUser.MessageTracking) { return; }
        if (!localGuild.MessageTracking) { return; }
        if (!localGuildUser.MessageTracking) { return; }
        
        // Update the statistics
        localChannelUser.MessagesSent++;
    }

    public static async Task ReactionsTask(DiscordClient client, MessageCreateEventArgs eventArgs, Database.Database database)
    {
        Console.WriteLine($"ReactionsTask for {eventArgs.Author.Username}");
        
        // Get the required services
        PublicHandler handler = database.Handlers.Public;
        ReactionsHandler reactionsHandler = database.Handlers.Reactions;
        
        // Get the user
        PublicUser publicUser = await handler.Users.Get(eventArgs.Author);

        // Get the reactions for the user 
        IEnumerable<ReactionsReaction> reactions = await reactionsHandler.GetReactions(publicUser.Id);
        
        // Try add the reaction to the message
        foreach (ReactionsReaction reaction in reactions)
        {
            Console.WriteLine($"Adding reaction {reaction.Emoji}({reaction.EmojiId}) to message.");

            bool success = reaction.TryGetEmoji(client, out DiscordEmoji? emoji);

            if (!success)
            {
                await reaction.Delete();
                continue;
            }
            
            // Add the reaction
            await eventArgs.Message.CreateReactionAsync(emoji);
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