using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Utilities;

public class Moderation : ApplicationCommandModule
{
    [SlashCommand("offtopic", "Moves off-topic chat to another channel")]
    [SlashCommandPermissions(Permissions.ManageMessages)]
    public async Task OfftopicMove(InteractionContext ctx,
        [Option("channel", "Channel to copy last messages to.")]
        DiscordChannel channel,
        [Option("limit", "Maximum amount of messages to copy")]
        string limit = "20")
    {
        if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.ManageMessages)
            && !channel.PermissionsFor(ctx.Member).HasPermission(Permissions.ManageMessages))
        {
            await ctx.CreateResponseAsync($"⚠️ Either of us does not have `MANAGE_MESSAGES` permission in {channel.Mention}!");
            return;
        }
        if (!int.TryParse(limit, out var parsedLimit) && parsedLimit < 1)
        {
            await ctx.CreateResponseAsync($"⚠️ {limit} is not a valid limit value!");
            return;
        }

        if (parsedLimit > 25)
        {
            await ctx.CreateResponseAsync("⚠️ The maximum limit for off-topic messages is 25!");
            return;
        }

        IEnumerable<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync(parsedLimit);

        await ctx.CreateResponseAsync($"⚠️ Your current conversation is off-topic! The most recent {parsedLimit} messages have been copied to {channel.Mention}.");

        await channel.SendMessageAsync($"⚠️ Copying off-topic messages from {ctx.Channel.Mention}!");
        var webhook = await channel.CreateWebhookAsync($"offtopic-move-{new Random().Next()}");

        foreach (var message in messages.Reverse())
        {
            if (string.IsNullOrEmpty(message.Content))
                continue;

            var webhookMessage = new DiscordWebhookBuilder()
                .WithContent(message.Content)
                .WithAvatarUrl(message.Author.GetAvatarUrl(ImageFormat.Auto))
                .WithUsername((message.Author as DiscordMember)?.DisplayName ?? string.Empty);
            
            await webhook.ExecuteAsync(webhookMessage);
        }
        await webhook.DeleteAsync();
        await channel.SendMessageAsync($"⚠ Off topic chat has been copied from {ctx.Channel.Mention}! Please continue conversation here.");
    }
    
    [SlashCommand("hackban", "Bans a user by ID. This user does not have to be part of this server.")]
    [SlashCommandPermissions(Permissions.BanMembers)]
    public async Task Hackban(InteractionContext ctx,
        [Option("userid", "ID of the user to ban.")] string userId,
        [Option("reason", "Reason to ban this user.")] string reason = null)
    {
        if (!ulong.TryParse(userId, out var id))
        {
            await ctx.CreateResponseAsync("⚠️ Invalid ID", true);
            return;
        }

        try
        {
            await ctx.Guild.BanMemberAsync(id, 7, reason);
            await ctx.CreateResponseAsync($"User with ID {userId} was banned.");
        }
        catch (Exception)
        {
            await ctx.CreateResponseAsync($"⚠️ Failed to ban user with ID {userId}.", true);
        }
    }
    
     [SlashCommand("quarantine", "Creates a new private thread with a user for moderation.")]
        [SlashCommandPermissions(Permissions.ManageThreads | Permissions.CreatePrivateThreads | Permissions.KickMembers)]
        public async Task quarantine(InteractionContext ctx,
            [Option("user", "User to isolate.")] DiscordUser user,
            [Option("reason", "Reason to isolate said member.")] string reason = null,
            [Option("user_2", "Add another user to isolate")] DiscordUser user2 = null,
            [Option("user_3", "Add another user to isolate")] DiscordUser user3 = null,
            [Option("user_4", "Add another user to isolate")] DiscordUser user4 = null,
            [Option("user_5", "Add another user to isolate")] DiscordUser user5 = null)
        {
            List<DiscordUser> users = new List<DiscordUser>();
            users.Add(user);
            users.Add(user2);
            users.Add(user3);
            users.Add(user4);
            users.Add(user5);
            users.RemoveAll(x => x == null);

            foreach (var cuser in users)
            {
                if (!(cuser as DiscordMember).PermissionsIn(ctx.Channel).HasPermission(Permissions.AccessChannels))
                {
                    await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                        .WithContent("⛔ One of the selected members can not access this channel. Try executing the command somewhere else!")
                        .AsEphemeral());
                    return;
                }
            }

            var mentions = string.Join(", ", users.Select(x => x.Mention));
            var names = string.Join(", ", users.Select(x => x.Username));

            var thread = await ctx.Channel.CreateThreadAsync($"⚠ {names}", AutoArchiveDuration.Day, ChannelType.PrivateThread, reason);
            await thread.SendMessageAsync($"⚠ {mentions}, a moderator has created an isolated chat with you" +
                $"{(reason != null ? $" for the following reasoning:\n```\n{reason}\n```" : ".")}" +
                "_Said moderator can bring in more members through_ ***@pinging*** _when needed._");

            // Adds the responsible moderator through a ping
            await (await thread.SendMessageAsync(ctx.Member.Mention)).DeleteAsync();

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .WithContent($"✅ Created a new private thread: {thread.Mention}")
                .AsEphemeral());
        }
        
        
        
}