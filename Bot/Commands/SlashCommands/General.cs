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
        DiscordActivity activity = new DiscordActivity
        {
            ActivityType = type,
            Name = status
        };
        
        // Get the config service
        ConfigHandler configHandler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Config;

        // Update the status in the database
        ConfigData data = await configHandler.NGet("status");
        data.Value = status;
        
        // Update the status type in the database
        ConfigData typeData = await configHandler.NGet("status_type");
        typeData.Value = ((int) type).ToString();
        
        await ctx.Client.UpdateStatusAsync(activity);
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = $"Status set to '{type} {status}'"
            });
    }
}