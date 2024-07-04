using Microsoft.Extensions.Configuration;

namespace Bot.Configuration;

/// <summary>
///     Bot configuration.
/// </summary>
public class Bot(string token, ulong testingChannel)
{
    /// <summary>
    ///     Bot token.
    /// </summary>
    public readonly string Token = token;
    public readonly ulong TestingChannel = testingChannel;
}

public class Logging(ulong logsChannel)
{
    public readonly ulong LogsChannel = logsChannel;
}

/// <summary>
///     Database configuration.
/// </summary>
public class DatabaseConfig(string host, int port, string username, string name, string password)
{
    public readonly string Host = host;
    public readonly string Name = name;
    public readonly string Password = password;
    public readonly int Port = port;
    public readonly string Username = username;
}

public class Config(IConfiguration config)
{
    public Bot Bot { get; } = new(
        config["Bot:Token"] ?? throw new MissingFieldException("Bot:Token"),
        ulong.Parse(config["Bot:TestingChannel"] ?? throw new MissingFieldException("Bot:TestingChannel"))
    );

    public Logging Logging { get; } = new(
        ulong.Parse(config["Logging:LogsChannel"] ?? throw new MissingFieldException("Logging:LogsChannel"))
    );
    
    public DatabaseConfig Database { get; } = new(
        config["Database:Host"] ?? throw new MissingFieldException("Database:Host"),
        int.Parse(config["Database:Port"] ?? throw new MissingFieldException("Database:Port")),
        config["Database:Username"] ?? throw new MissingFieldException("Database:Username"),
        config["Database:Name"] ?? throw new MissingFieldException("Database:Name"),
        config["Database:Password"] ?? throw new MissingFieldException("Database:Password")
    );
}