using System.Reflection;
using Bot.Commands.SlashCommands;
using Bot.Configuration;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bot;

public class Program
{
    private static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private async Task MainAsync()
    {
        DotEnv.Load(new FileInfo(".env"));
        Config config = new(
            new ConfigurationBuilder()
                .AddJsonFile("config/appsettings.json")
                .AddEnvironmentVariables()
                .Build()
        );

        Database.Database database = new(
            config.Database.Host,
            config.Database.Port,
            config.Database.Username,
            config.Database.Name,
            config.Database.Password
        );

        // Create a service provider
        IServiceProvider serviceProvider = new ServiceCollection() // Add database and config as singletons 
            .AddSingleton(config)
            .AddSingleton(database)
            .BuildServiceProvider();

        // Create a new Discord client
        DiscordClient discord = new(new DiscordConfiguration
        {
            Token = config.Bot.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.Guilds | DiscordIntents.GuildMessages |
                      DiscordIntents.GuildModeration | DiscordIntents.MessageContent,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug,
            ServiceProvider = serviceProvider
        });

        discord.RegisterEventHandlers(Assembly.GetExecutingAssembly());

        // Register commands
        ApplicationCommandsExtension commands = discord.UseApplicationCommands(new ApplicationCommandsConfiguration
        {
            ServiceProvider = serviceProvider
        });

        commands.RegisterGlobalCommands<GeneralCommands>();
        commands.RegisterGlobalCommands<StatisticsCommands>();
        commands.RegisterGlobalCommands<PrivacyCommands>();
        commands.RegisterGlobalCommands<ReactionsCommands>();
        commands.RegisterGuildCommands<Testing>(config.Bot.TestingChannel);

        commands.SlashCommandErrored += async (extension, args) =>
        {
            ErrorHandler.Handle(args.Exception, args.Context);

            // Let the user know that the command errored
            await args.Context.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    Content =
                        "An error occured while executing the command. The error has been logged and the devs notified."
                });

            // Edit the response to show the error, this is done in case the command errored after it was already responded to
            await args.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "An error occurred while executing the command. The error has been logged and the devs notified."));

            await Task.CompletedTask;
        };

        // Run bot
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}