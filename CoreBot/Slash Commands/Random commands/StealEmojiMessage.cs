using System.Diagnostics;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CoreBot.Slash_Commands.Random_commands;

public class StealEmojiMessage : ApplicationCommandModule
{
    private readonly Stopwatch _executionTimer = new();
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

    [ContextMenu(ApplicationCommandType.MessageContextMenu, "StealEmoji"),
     SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task Yoink(ContextMenuContext ctx)
    {
        await ctx.DeferAsync();
        
        var matches = Regex.Matches(ctx.TargetMessage.Content.Replace("><", "> <"), EmojiRegex, RegexOptions.Compiled);
        if (matches.Count < 1)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Emoji not found!"));
            return;
        }
        var distinctMatches = matches.DistinctBy(x => x.Value).ToList();
        var newEmojis = new List<DiscordEmoji>();

        foreach (var match in distinctMatches)
        {
            try
            {
                var split = match.Groups[2].Value;
                var emojiName = match.Groups[1].Value;
                var animated = match.Value.StartsWith("<a");
                if (!ulong.TryParse(split, out var emojiId))
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent("Failed to fetch your new emoji"));
                var success = await CopyEmoji(ctx, emojiName, emojiId, animated);
                
                newEmojis.Add(success);

            }
            catch (Exception)
            {
                //ignored, lol
            }
            await IntendedWait(1000);
        }
        
        var message = newEmojis.Aggregate("Yoink. These emoji(s) have been added to your server: ",
            (current, emoji) => current + $" {emoji}");
        message += $" {newEmojis.Count}/{distinctMatches.Count} emojis added";

        var discordWebhook = new DiscordWebhookBuilder().AddEmbed(
            new DiscordEmbedBuilder().WithTitle(message));

        await ctx.EditResponseAsync(discordWebhook);
        
    }
    
    private static async Task<DiscordEmoji> CopyEmoji(ContextMenuContext ctx, string name, ulong id, bool animated)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");

        MemoryStream memory = new();

        await downloadedEmoji.CopyToAsync(memory);

        await downloadedEmoji.DisposeAsync();
        var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);

        return newEmoji;
    }
    
    private async Task IntendedWait(int milliseconds)
    {
        _executionTimer.Stop();

        await Task.Delay(milliseconds);

        _executionTimer.Start();
    }
}