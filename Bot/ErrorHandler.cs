using DisCatSharp;
using DisCatSharp.ApplicationCommands.Context;

namespace Bot;

public class ErrorHandler
{
    /// <summary>
    /// Path to the error log file.
    /// </summary>
    private static readonly FileInfo ErrorLogPath = new("error.log");

    /// <summary>
    /// Handles any exceptions that occur in the bot. Writes exception details to a file.
    /// </summary>
    /// <param name="exception">Exception to handle.</param>
    /// <param name="context">Context, used if logging to a dedicated channel is desired.</param>
    public static void Handle(Exception exception, BaseContext? context = null)
    {
        // Open the error log file
        using StreamWriter writer = ErrorLogPath.AppendText();
        
        // Write the exception details to the file
        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception.Message}");
        writer.WriteLine(exception.StackTrace);
        writer.WriteLine();
        
        // Log the exception to the console
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception.Message}");
        Console.WriteLine(exception.StackTrace);

        if (context is not null)
        {
            // Get the channel to log to
            
        }
    }
}