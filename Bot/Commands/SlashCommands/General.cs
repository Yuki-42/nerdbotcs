using Bot.Database.Handlers.Config;
using Bot.Database.Types.Config;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.SlashCommands;

public class GeneralCommands : ApplicationCommandsModule
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

    [SlashCommand("status", "Set the bot status.")]
    public async Task StatusCommand(
        InteractionContext ctx,
        [Option("status", "The status to set.")]
        string status,
        [Option("type", "The type of status to set.")]
        ActivityType type
    )
    {
        DiscordActivity activity = new()
        {
            ActivityType = type,
            Name = status
        };

        // Get the config service
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Config;

        // Update the status in the database
        ConfigRow row = await handler.NGet("status");
        row.Value = status;

        // Update the status type in the database
        ConfigRow typeRow = await handler.NGet("status_type");
        typeRow.Value = ((int)type).ToString();

        await ctx.Client.UpdateStatusAsync(activity);
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = $"Status set to '{type} {status}'"
            });
    }
}