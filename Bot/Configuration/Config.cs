using Microsoft.Extensions.Configuration;

namespace Bot.Configuration;

/// <summary>
///  Bot configuration.
/// </summary>
public class Bot(string token, ulong testingGuild, int maxReacts)
{
	/// <summary>
	/// Guild ID for bot debug.
	/// </summary>
	public readonly ulong TestingChannel = testingGuild;

    /// <summary>
    ///  Bot token.
    /// </summary>
    public readonly string Token = token;

    /// <summary>
    /// Maximum number of reactions applicable to a user at any given time.
    /// </summary>
    public readonly int MaxReacts = maxReacts;
}

public class Logging(ulong logsChannel)
{
	public readonly ulong LogsChannel = logsChannel;
}

/// <summary>
///  Database configuration.
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
		ulong.Parse(config["Bot:TestingGuild"] ?? throw new MissingFieldException("Bot:TestingChannel")),
		int.Parse(config["Bot:MaxReacts"] ?? throw new MissingFieldException("Bot:MaxReacts"))
	);

	public Logging Logging { get; } = new(
		ulong.Parse(config["Logging:Channel"] ?? throw new MissingFieldException("Logging:LogsChannel"))
	);

	public DatabaseConfig Database { get; } = new(
		config["Database:Host"] ?? throw new MissingFieldException("Database:Host"),
		int.Parse(config["Database:Port"] ?? throw new MissingFieldException("Database:Port")),
		config["Database:Username"] ?? throw new MissingFieldException("Database:Username"),
		config["Database:Name"] ?? throw new MissingFieldException("Database:Name"),
		config["Database:Password"] ?? throw new MissingFieldException("Database:Password")
	);
}