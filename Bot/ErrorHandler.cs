using Bot.Configuration;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Bot;

public class ErrorHandler
{
    /// <summary>
    ///     Path to the error log file.
    /// </summary>
    private static readonly FileInfo ErrorLogPath = new("error.log");

    /// <summary>
    ///     Handles any exceptions that occur in the bot. Writes exception details to a file.
    /// </summary>
    /// <param name="exception">Exception to handle.</param>
    /// <param name="context">Context, used if logging to a dedicated channel is desired.</param>
    public static async void Handle(Exception exception, BaseContext? context = null)
    {
        // Open the error log file
        await using StreamWriter? writer = ErrorLogPath.AppendText();

        // Create error message 
        string? errorMessage =
            $"========================================\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n `{exception.Message}`\n```{exception.StackTrace}```\n========================================";

        // Write the exception details to the file
        await writer.WriteLineAsync(errorMessage);

        // Log the exception to the console
        Console.WriteLine(errorMessage);

        if (context is null) return;
        // Get the config service
        Config? config = context.Services.GetRequiredService<Config>();

        // Get the channel to log to
        DiscordChannel? channel = await context.Client.GetChannelAsync(config.Logging.LogsChannel);

        // Try and send the error message to the channel
        try
        {
            await channel.SendMessageAsync(
                new DiscordMessageBuilder()
                    .WithContent(errorMessage)
            );
        }
        catch (Exception e)
        {
            // If an error occurs while sending the error message, log it to the console
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
    }
}