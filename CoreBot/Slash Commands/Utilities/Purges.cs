﻿using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CoreBot.Slash_Commands.Utilities;

[SlashCommandGroup("purge", "Commands for clearing chat.")]
[SlashCommandPermissions(Permissions.ManageMessages)]
[SlashRequirePermissions(Permissions.ManageMessages)]

public class Purges : ApplicationCommandModule
{
    private async Task deleteMessagesAsync(InteractionContext ctx, IEnumerable<DiscordMessage> messages)
    {
        var deleteable = messages;
        deleteable = deleteable.Where(x => DateTimeOffset.Now.Subtract(x.CreationTimestamp).TotalDays < 14).ToList();

        if(!deleteable.Any())
        {
            await ctx.CreateResponseAsync("⚠️ No messages were deleted. Take note that messages older than 14 days can not be deleted with purge", true);
            return;
        }

        await ctx.Channel.DeleteMessagesAsync(deleteable, "NineBot Purge");
        await ctx.CreateResponseAsync($"✅ Deleted {deleteable.Count()} messages.", true);
    }
    
    [SlashCommand("regular", "Clears chat without any special parameters.")]
    public async Task RegularPurgeAsync(InteractionContext ctx, 
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)]long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)]long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);

        await deleteMessagesAsync(ctx, messages);
    }
    
    [SlashCommand("user", "Clears chat by a specific user.")]
    public async Task UserPurgeAsync(InteractionContext ctx, 
        [Option("user", "User to delete messages from.")]DiscordUser user,
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)] long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)] long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);
        messages = messages.Where(x => x.Author.Id == user.Id);

        await deleteMessagesAsync(ctx, messages);
    }
    
    [SlashCommand("regex", "Clears chat using a Regular Expression. Don't use this if you don't know how to")]
    public async Task RegexPurgeAsync(InteractionContext ctx, 
        [Option("regex", "Regular Expression to use.")] string regex,
        [Option("flags", "Regex flags to use.")] string flags = "",
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)] long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)] long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);

        var regexOptions = RegexOptions.CultureInvariant;

        if (string.IsNullOrEmpty(regex))
        {
            await ctx.CreateResponseAsync("⚠️ Regex is empty!", true);
            return;
        }

        if (flags.Contains('m'))
        {
            regexOptions |= RegexOptions.Multiline;
        }
        if (flags.Contains('i'))
        {
            regexOptions |= RegexOptions.IgnoreCase;
        }
        if (flags.Contains('s'))
        {
            regexOptions |= RegexOptions.Singleline;
        }
        if (flags.Contains('x'))
        {
            regexOptions |= RegexOptions.ExplicitCapture;
        }
        if (flags.Contains('r'))
        {
            regexOptions |= RegexOptions.RightToLeft;
        }

        Regex regexCompiled;
        try
        {
            regexCompiled = new Regex(regex, regexOptions);
        }
        catch(Exception)
        {
            await ctx.CreateResponseAsync($"⚠️ Invalid Regex!");
            return;
        }

        messages = messages.Where(x => regexCompiled.IsMatch(x.Content));
        await deleteMessagesAsync(ctx, messages);
    }
    
    [SlashCommand("bots", "Clears chat messages by bots.")]
    public async Task BotsPurgeAsync(InteractionContext ctx,
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)] long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)] long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);
        messages = messages.Where(x => x.Author.IsBot);

        await deleteMessagesAsync(ctx, messages);
    }
    
    [SlashCommand("attachments", "Clears chat messages with attachments.")]
    public async Task AttachmentsPurgeAsync(InteractionContext ctx,
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)] long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)] long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);
        messages = messages.Where(x => x.Attachments.Count > 0);

        await deleteMessagesAsync(ctx, messages);
    }
    
    private Regex ImageRegex = new Regex(@"\.(png|gif|jpg|jpeg|tiff|webp)");
    [SlashCommand("images", "Clears chat messages with attachments.")]
    public async Task ImagesPurgeAsync(InteractionContext ctx,
        [Option("limit", "Maximum amount of messages to fetch in this Purge")][Maximum(100)][Minimum(1)] long limit = 50,
        [Option("skip", "Amount of newer messages to skip when purging")][Minimum(0)][Maximum(99)] long skip = 0)
    {
        IEnumerable<DiscordMessage> messages = (await ctx.Channel.GetMessagesAsync((int)limit)).Skip((int)skip);
        messages = messages.Where(x => x.Attachments.Count > 0);
        messages = messages.Where(m => ImageRegex.IsMatch(m.Content) || m.Attachments.Any(x => ImageRegex.IsMatch(x.FileName)));
        await deleteMessagesAsync(ctx, messages);
    }

}