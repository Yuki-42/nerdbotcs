namespace Bot.Database.Handlers.Public;

public class PublicHandler(string connectionString) : BaseHandler(connectionString)
{
    public PublicGuildsHandler Guilds { get; private set; } = new(connectionString);
    public PublicUsersHandler Users { get; private set; } = new(connectionString);
    public PublicChannelsHandler Channels { get; private set; } = new(connectionString);
    public PublicGuildsUsersHandler GuildUsers { get; private set; } = new(connectionString);
    public PublicChannelsUsersHandler ChannelUsers { get; private set; } = new(connectionString);
}