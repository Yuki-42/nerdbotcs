using System.Diagnostics;
using Bot.Database.Handlers.Public;
using Bot.Database.Types.Public;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Commands.Logic;

public class Statistics
{
    /// <summary>
    ///     Stores the completion messages for global audit.
    /// </summary>
    private static readonly string[] AuditCompletion =
    [
        "Running %s audit, this may take a while.\n- :red_square: Users\n- :red_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
        "Running %s audit, this may take a while.\n- :green_square: Users\n- :red_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
        "Running %s audit, this may take a while.\n- :green_square: Users\n- :green_square: Servers\n- :red_square: Channels\n- :red_square: Messages",
        "Running %s audit, this may take a while.\n- :green_square: Users\n- :green_square: Servers\n- :green_square: Channels\n- :red_square: Messages",
        "%s audit completed.\n- :green_square: Users\n- :green_square: Guilds\n- :green_square: Channels\n- :green_square: Messages"
    ];

    /// <summary>
    ///     Audit all categories as a global admin.
    /// </summary>
    /// <param name="ctx">Context.</param>
    public static async Task AuditAllGlobalAdmin(BaseContext ctx)
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = AuditCompletion[0].Replace("%s", "global")
            });

        await AuditAllUsers(ctx);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[1].Replace("%s", "global")
            });

        await AuditAllGuilds(ctx);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[2].Replace("%s", "global")
            });

        await AuditAllChannels(ctx);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[3].Replace("%s", "global")
            });

        await AuditAllMessages(ctx);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[4].Replace("%s", "Global")
            });
    }


    /// <summary>
    ///     Audit all categories as a server admin.
    /// </summary>
    /// <param name="ctx">Context.</param>
    /// <exception cref="InvalidOperationException">Thrown when a null guild is audited.</exception>
    public static async Task AuditAllServerAdmin(BaseContext ctx)
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = AuditCompletion[0].Replace("%s", "server")
            });

        if (ctx.Guild is null) throw new InvalidOperationException("Cannot audit a null guild.");

        await AuditGuildUsers(ctx, ctx.Guild);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[1].Replace("%s", "server")
            });

        await AuditGuild(ctx, ctx.Guild);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[2].Replace("%s", "server")
            });

        await AuditGuildChannels(ctx, ctx.Guild);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[3].Replace("%s", "server")
            });
        await AuditGuildMessages(ctx, ctx.Guild);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder
            {
                Content = AuditCompletion[4].Replace("%s", "server")
            });
    }

    /// <summary>
    ///     Audit all guilds.
    /// </summary>
    /// <param name="ctx">Context</param>
    public static async Task AuditAllGuilds(BaseContext ctx)
    {
        // Get the required handlers
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all guilds
        DiscordGuild[] guilds = ctx.Client.Guilds.Values.ToArray();

        foreach (DiscordGuild guild in guilds) await AuditGuild(ctx, guild, handler);

        // Check that there are no guilds in the database that are not in the bot
        IEnumerable<GuildsRow> publicGuilds = await handler.Guilds.GetAll();
        foreach (GuildsRow publicGuild in publicGuilds)
            if (guilds.All(guild => guild.Id != publicGuild.Id))
                await publicGuild.Delete();
    }

    /// <summary>
    ///     Audits a specific guild.
    /// </summary>
    /// <param name="ctx">Context</param>
    /// <param name="guild">Guild</param>
    /// <param name="handler">Handler</param>
    public static async Task AuditGuild(BaseContext ctx, DiscordGuild? guild, Handler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers if not provided
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Check if the guild exists in the database
        GuildsRow publicGuild = await handler.Guilds.Get(guild);

        // Update the guild name if it's different
        if (publicGuild.Name != guild.Name) publicGuild.Name = guild.Name; // This updates the name in the database through the setter

        // Add all users in the guild
        IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
        foreach (DiscordMember member in members)
        {
            // Check if the user exists in the database
            UsersRow user = await handler.Users.Get(member); // This creates a new user if it doesn't exist

            // Update the username if it's different
            if (user.Username != member.Username) user.Username = member.Username; // This updates the username in the database through the setter
        }
    }

    /// <summary>
    ///     Audit all channels.
    /// </summary>
    /// <param name="ctx">Context</param>
    public static async Task AuditAllChannels(BaseContext ctx)
    {
        // Get the required handlers
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all guilds
        DiscordGuild[] guilds = ctx.Client.Guilds.Values.ToArray();

        foreach (DiscordGuild guild in guilds) await AuditGuildChannels(ctx, guild, handler);
    }

    /// <summary>
    ///     Audits all channels in a specific guild.
    /// </summary>
    /// <param name="ctx">Context</param>
    /// <param name="guild">Guild</param>
    /// <param name="handler">Handler</param>
    public static async Task AuditGuildChannels(BaseContext ctx, DiscordGuild? guild, Handler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all channels in the guild
        IReadOnlyCollection<DiscordChannel> channels = await guild.GetChannelsAsync();
        foreach (DiscordChannel channel in channels)
        {
            // Check if the channel exists in the database
            ChannelsRow publicChannel = await handler.Channels.Get(channel);

            // Update the name if it's different
            if (publicChannel.Name != channel.Name) publicChannel.Name = channel.Name; // This updates the name in the database through the setter

            // Update the type if it's different
            if (publicChannel.Type != channel.Type) publicChannel.Type = channel.Type; // This updates the type in the database through the setter
        }

        // Check that there are no channels in the database that are not in the bot
        IEnumerable<ChannelsRow> publicChannels = await handler.Channels.GetAll(guild.Id);
        foreach (ChannelsRow publicChannel in publicChannels)
            if (channels.All(channel => channel.Id != publicChannel.Id))
                await publicChannel.Delete();
    }

    /// <summary>
    ///     Audit all users.
    /// </summary>
    /// <param name="ctx">Context</param>
    public static async Task AuditAllUsers(BaseContext ctx)
    {
        // Get the required handler
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // First get all servers that the bot is in
        DiscordGuild?[] guilds = ctx.Client.Guilds.Values.ToArray();

        // Get all users
        IEnumerable<DiscordMember> members = [];
        // ReSharper disable once LoopCanBeConvertedToQuery  // This is a bu_g in ReSharper, it doesn't understand the await keyword // also apparently the word bu-g is a to-do item
        foreach (DiscordGuild? guild in guilds)
        {
            if (guild is null) continue;
            members = members.Concat(await guild.GetAllMembersAsync());
        }

        foreach (DiscordGuild? guild in guilds) await AuditGuildUsers(ctx, guild, handler);

        // Check that there are no users in the database that are not in the bot
        IEnumerable<UsersRow> publicUsers = await handler.Users.GetAll();

        // Get a list of all users in the bot
        foreach (UsersRow publicUser in publicUsers)
            if (members.All(member => member.Id != publicUser.Id))
                await publicUser.Delete();
    }

    public static async Task AuditGuildUsers(BaseContext ctx, DiscordGuild? guild, Handler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
        foreach (DiscordMember member in members)
        {
            // Check if the user exists in the database // This is already done in the New method
            UsersRow user = await handler.Users.Get(member);

            // Update the username if it's different
            if (user.Username != member.Username) user.Username = member.Username; // This updates the username in the database through the setter
        }
    }

    /// <summary>
    ///     Counts the messages in a block of messages.
    /// </summary>
    /// <param name="messages">Message block.</param>
    /// <param name="users">Object to count into.</param>
    /// <param name="userMessageTracking">User Message tracking cache.</param>
    private static void CountMessagesBlock(IEnumerable<DiscordMessage> messages, ref Dictionary<DiscordUser, long> users, ref Dictionary<DiscordUser, bool> userMessageTracking)
    {
        foreach (DiscordMessage message in messages)
        {
            // Check if the user is a deleted user
            userMessageTracking.TryAdd(message.Author, false);

            // Check if user has message tracking enabled
            if (!userMessageTracking[message.Author]) continue;

            // Update the message count
            if (!users.TryAdd(message.Author, 1)) users[message.Author]++;
        }
    }

    public static async Task AuditAllMessages(BaseContext ctx)
    {
        // Note: This is a very expensive operation and should be used sparingly.

        // Get the required handlers
        Handler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all guilds
        DiscordGuild[] guilds = ctx.Client.Guilds.Values.ToArray();

        foreach (DiscordGuild guild in guilds)
            try
            {
                await AuditGuildMessages(ctx, guild, handler);
            }
            catch (Exception exception)
            {
                ErrorHandler.Handle(exception, ctx);
            }
    }

    public static async Task AuditGuildMessages(BaseContext ctx, DiscordGuild? guild, Handler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // First check if message tracking is enabled for the guild
        GuildsRow publicGuild = await handler.Guilds.Get(guild);
        if (!publicGuild.MessageTracking) return;

        // Lookup table for message tracking on a per-user basis
        Dictionary<DiscordUser, bool> userMessageTracking = new();
        foreach (DiscordMember member in await guild.GetAllMembersAsync())
        {
            UsersRow user = await handler.Users.Get(member);
            GuildsUsersRow guildUser = await handler.GuildUsers.Get(member, guild);
            userMessageTracking.Add(member, user.MessageTracking && guildUser.MessageTracking);
        }

        // Get all channels in the guild
        IReadOnlyCollection<DiscordChannel> channels = await guild.GetChannelsAsync();
        foreach (DiscordChannel channel in channels)
        {
            // Check if channel is a text channel
            if (channel.Type != ChannelType.Text) continue;

            Console.WriteLine($"Auditing channel {channel.Name} in guild {guild.Name}.");

            // Check if message tracking is enabled for the channel
            ChannelsRow publicChannel = await handler.Channels.Get(channel);
            if (!publicChannel.MessageTracking) continue;

            // Check if the bot has permissions to read messages
            DiscordMember bot = await guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (!channel.PermissionsFor(bot).HasPermission(Permissions.AccessChannels)) continue;

            // Store the user's message count
            Dictionary<DiscordUser, long> users = new();

            // Store the most recent 1000 message ids, this is to prevent duplicates
            HashSet<ulong> messageIds = [];

            // Count the messages
            ulong oldestId = 0;
            while (true)
            {
                IReadOnlyList<DiscordMessage> messages;
                try
                {
                    messages = oldestId != 0
                        ? await channel.GetMessagesBeforeAsync(oldestId)
                        : await channel.GetMessagesAsync();
                }
                catch (ArgumentException exception)
                {
                    ErrorHandler.Handle(exception, ctx);
                    continue;
                }

                // Check if there are no messages
                if (messages.Count <= 0) break;

                // Check if any message is a duplicate
                foreach (DiscordMessage message in messages)
                {
                    Debug.Assert(!messageIds.Contains(message.Id), "Duplicate message found.");
                    messageIds.Add(message.Id);
                }

                CountMessagesBlock(messages, ref users, ref userMessageTracking);

                oldestId = messages[^1].Id;
            }

            // Update the message count for each user
            foreach (KeyValuePair<DiscordUser, long> user in users)
            {
                ChannelsUsersRow guildUser = await handler.ChannelUsers.Get(user.Key, channel);
                guildUser.MessagesSent = user.Value;
            }
        }
    }
}