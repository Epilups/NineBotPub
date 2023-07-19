using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Utilities;

public class Status : ApplicationCommandModule
{
    private static Regex nameRegex = new("PRETTY_NAME=(.*)", RegexOptions.Compiled);

    private string fetchLinuxName()
    {
        try
        {
            var result = File.ReadAllText("/etc/os-release");
            var match = nameRegex.Match(result);
            if (!match.Success)
                return Environment.OSVersion.VersionString;
            return match.Groups[1].Value.Replace("\"", "");
        }catch(Exception)
        {
            return Environment.OSVersion.VersionString;
        }
    }

        [SlashCommand("status", "Returns NineBot status info")]
    public async Task GetStatus(InteractionContext ctx)
    {
        var osVer = Environment.OSVersion.VersionString;
        var embed = new DiscordEmbedBuilder()
            .WithTitle("NineBot Status")
            .WithDescription("Information about NineBot")
            .WithColor(DiscordColor.Azure)
            .WithThumbnail(ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png))
            .AddField("Socket ping", $"{ctx.Client.Ping}", true)
            .AddField("Shards", $"{ctx.Client.ShardCount}", true)
            .AddField("Current Shard", $"{ctx.Client.ShardId}", true)
            .AddField("Operating System",
                osVer.StartsWith("Unix") ? fetchLinuxName() : Environment.OSVersion.VersionString, true)
            .AddField("Framework", RuntimeInformation.FrameworkDescription, true);
        
        await ctx.CreateResponseAsync(embed);

    }
}