using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Bot.Commands.SlashCommands;

public class Testing : ApplicationCommandsModule
{
    [SlashCommand("channel", "Gets channel info.")]
    public static async Task GetChannelCommand(
        InteractionContext ctx,
        [Option("channel", "The channel to get info from.")]
        DiscordChannel channel
    )
    {
        // Get channel from id
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = $"Channel: {channel.Name}\nGuild: {channel.Guild.Name}"
            });
    }
}