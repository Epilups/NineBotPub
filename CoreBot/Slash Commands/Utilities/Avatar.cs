using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Utilities;

public class Avatar : ApplicationCommandModule
{
    [SlashCommand("avatar", "Fetches a user's avatar with URL.")]
    public async Task AvatarAsync(InteractionContext ctx, [Option("user", "User to fetch the avatar from.")] DiscordUser user)
    {
        var img = user.GetAvatarUrl(ImageFormat.Png, 4096);
        
        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Avatar for user {user.Username}")
            .WithImageUrl(img);

        await ctx.CreateResponseAsync(embed);
    }
}