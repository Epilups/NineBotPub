using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Utilities;

public class Avatar : ApplicationCommandModule
{
    [SlashCommand("avatar",
        "Returns the avatar of the provided user. Defaults to yourself if no user is provided.")]
    public static async Task AvatarCommand(InteractionContext ctx, [Option("user", "The user whose avatar to get.")] 
        DiscordUser? user = null)
    {
        user ??= ctx.User;

        DiscordMember? member = default;
        try
        {
            member = await ctx.Guild.GetMemberAsync(user.Id);
        }
        catch
        {
            // ignored
        }

        if (member == default || member.GuildAvatarUrl is null)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent($"{user.AvatarUrl}".Replace("size=1024", "size=4096")));
        }
    }
}