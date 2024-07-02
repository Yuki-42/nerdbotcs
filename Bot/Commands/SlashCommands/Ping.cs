using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Bot.Commands.SlashCommands;

public class PingCommands : ApplicationCommandsModule
{
    [SlashCommand("ping", "Bot ping to discord servers.")]
    public async Task PingCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = $"Pong! {ctx.Client.Ping}ms"
            });
    }
}