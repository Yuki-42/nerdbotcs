﻿using System.Diagnostics;
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
    ///     Audit all guilds.
    /// </summary>
    /// <param name="ctx">Context</param>
    public static async Task AuditAllGuilds(BaseContext ctx)
    {
        // Get the required handlers
        PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all guilds
        DiscordGuild[] guilds = ctx.Client.Guilds.Values.ToArray();

        foreach (DiscordGuild guild in guilds) await AuditGuild(ctx, guild, handler);
    }

    /// <summary>
    ///     Audits a specific guild.
    /// </summary>
    /// <param name="ctx">Context</param>
    /// <param name="guild">Guild</param>
    /// <param name="handler">Handler</param>
    public static async Task AuditGuild(BaseContext ctx, DiscordGuild? guild, PublicHandler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers if not provided
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Check if the guild exists in the database
        PublicGuild publicGuild = await handler.Guilds.Get(guild);

        // Update the guild name if it's different
        if (publicGuild.Name != guild.Name) publicGuild.Name = guild.Name; // This updates the name in the database through the setter

        // Add all users in the guild
        IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
        foreach (DiscordMember member in members)
        {
            // Check if the user exists in the database
            PublicUser user = await handler.Users.Get(member); // This creates a new user if it doesn't exist

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
        PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

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
    public static async Task AuditGuildChannels(BaseContext ctx, DiscordGuild? guild, PublicHandler? handler = null)
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
            PublicChannel publicChannel = await handler.Channels.Get(channel);

            // Update the name if it's different
            if (publicChannel.Name != channel.Name) publicChannel.Name = channel.Name; // This updates the name in the database through the setter

            // Update the type if it's different
            if (publicChannel.Type != channel.Type) publicChannel.Type = channel.Type; // This updates the type in the database through the setter
        }
    }

    /// <summary>
    ///     Audit all users.
    /// </summary>
    /// <param name="ctx">Context</param>
    public static async Task AuditAllUsers(BaseContext ctx)
    {
        // Get the required handler
        PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // First get all servers that the bot is in
        DiscordGuild?[] guilds = ctx.Client.Guilds.Values.ToArray();

        // Get all users
        foreach (DiscordGuild? guild in guilds) await AuditGuildUsers(ctx, guild, handler);
    }

    public static async Task AuditGuildUsers(BaseContext ctx, DiscordGuild? guild, PublicHandler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
        foreach (DiscordMember member in members)
        {
            // Check if the user exists in the database // This is already done in the New method
            PublicUser user = await handler.Users.Get(member);

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
        PublicHandler handler = ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // Get all guilds
        DiscordGuild[] guilds = ctx.Client.Guilds.Values.ToArray();

        foreach (DiscordGuild guild in guilds) await AuditGuildMessages(ctx, guild, handler);
    }

    public static async Task AuditGuildMessages(BaseContext ctx, DiscordGuild? guild, PublicHandler? handler = null)
    {
        // Check if the guild is null
        if (guild is null) return;

        // Get the required handlers
        handler ??= ctx.Services.GetRequiredService<Database.Database>().Handlers.Public;

        // First check if message tracking is enabled for the guild
        PublicGuild publicGuild = await handler.Guilds.Get(guild);
        if (!publicGuild.MessageTracking) return;

        // Lookup table for message tracking on a per-user basis
        Dictionary<DiscordUser, bool> userMessageTracking = new();
        foreach (DiscordMember member in await guild.GetAllMembersAsync())
        {
            PublicUser user = await handler.Users.Get(member);
            PublicGuildUser guildUser = await handler.GuildUsers.Get(member, guild);
            userMessageTracking.Add(member, user.MessageTracking && guildUser.MessageTracking);
        }

        // Get all channels in the guild
        IReadOnlyCollection<DiscordChannel> channels = await guild.GetChannelsAsync();
        foreach (DiscordChannel channel in channels)
        {
            // Check if channel is a text channel
            if (channel.Type != ChannelType.Text) continue;

            // Check if message tracking is enabled for the channel
            PublicChannel publicChannel = await handler.Channels.Get(channel);
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
                IReadOnlyList<DiscordMessage> messages = oldestId != 0
                    ? await channel.GetMessagesBeforeAsync(oldestId)
                    : await channel.GetMessagesAsync();

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
                PublicChannelUser guildUser = await handler.ChannelUsers.Get(user.Key, channel);
                guildUser.MessagesSent = user.Value;
            }
        }
    }
}