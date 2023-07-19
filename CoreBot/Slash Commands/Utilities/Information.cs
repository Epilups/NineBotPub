using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Utilities;

[SlashCommandGroup("information", "Lists information about specified entity")]
public class Information : ApplicationCommandModule

{
    [SlashCommand("member", "Information about a member")]
    public async Task MemberInfo(InteractionContext ctx,
        [Option("user", "User to show information about")]
        DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);

        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithTitle($"{member.Username} ({member.Id})");

        if (member.IsBot)
        {
            embed.Title += " (BOT) ";
        }

        if (member.IsOwner)
        {
            embed.Title += " (OWNER) ";
        }
        
        embed.Description =
            $"Registered: <t:{member.CreationTimestamp.ToUnixTimeSeconds()}:F>\n" +
            $"Joined: <t:{member.JoinedAt.ToUnixTimeSeconds()}:F>";

        var roles = new StringBuilder();
        foreach (var r in member.Roles)
            roles.Append($"`{r.Name.Replace("`", "'")}` ");

        if (roles.Length == 0)
            roles.Append("*None*");

        var permissionsEnum = member.Permissions;
        var permissions = permissionsEnum.ToPermissionString();

        embed.AddField("Roles", roles.ToString());
        embed.AddField("Permissions", permissions);
        embed.WithThumbnail(member.GetGuildAvatarUrl(ImageFormat.Auto));

        await ctx.CreateResponseAsync(embed);
        
    }
    
    [SlashCommand("permissions", "Information about user's permissions")]
    public async Task PermsInfo(InteractionContext ctx,
        [Option("user", "User to show information about.")] DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);

        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithTitle($"All permissions for {member.Username} ({member.Id})");

        embed.WithDescription(member.Permissions.ToPermissionString());
        embed.WithThumbnail(member.GetGuildAvatarUrl(ImageFormat.Auto));

        await ctx.CreateResponseAsync(embed);
    }
    
    [SlashCommand("channel-permissions", "Channel permissions for a specific user")]
    public async Task ChannelPermsInfo(InteractionContext ctx, [Option("user", "User to show information about.")] DiscordUser user)
    {
        var member = await ctx.Guild.GetMemberAsync(user.Id);

        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithTitle($"Permissions in current channel for {member.Username} ({member.Id})");

        var permissionsEnum = member.PermissionsIn(ctx.Channel);

        embed.WithDescription(permissionsEnum.ToPermissionString());
        embed.WithThumbnail(member.GetGuildAvatarUrl(ImageFormat.Auto));

        await ctx.CreateResponseAsync(embed);
    }
    
    [SlashCommand("role", "Information about a role.")]
    public async Task RoleAsync(InteractionContext ctx, [Option("role", "Role to show information about.")]DiscordRole role)
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle($"{role.Name} ID: ({role.Id})")
            .WithDescription($"Created at <t:{role.CreationTimestamp.ToUnixTimeSeconds()}:F>")
            .AddField("Permissions", role.Permissions.ToPermissionString())
            .AddField("Data", $"Mentionable: {(role.IsMentionable ? "yes" : "no")}.\nHoisted: {(role.IsHoisted ? "yes" : "no")}.\nManaged: {(role.IsManaged ? "yes" : "no")}.")
            .WithColor(role.Color);

        if (!string.IsNullOrEmpty(role.IconUrl))
            embed.WithThumbnail(role.IconUrl);

        if (!string.IsNullOrEmpty(role.IconUrl))
            embed.WithThumbnail(role.IconUrl);

        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("server", "Information about the current server.")]
    public async Task ServerAsync(InteractionContext ctx)
    {
        var server = ctx.Guild;
        var member = await server.GetMemberAsync(ctx.User.Id);
        
        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithTitle($"{server.Name} ID: ({server.Id})")
            .WithDescription(
                $"Created: <t:{server.CreationTimestamp.ToUnixTimeSeconds()}:F>\n" + 
                $"Member count: {server.MemberCount}\n" +
                $"Joined: <t:{server.JoinedAt.ToUnixTimeSeconds()}:F>\n" +
                $"You joined: <t:{member.JoinedAt.ToUnixTimeSeconds()}:F>");

        if (!string.IsNullOrEmpty(server.IconHash))
            embed.WithThumbnail(server.IconUrl);
        
        embed.WithAuthor($"Owner: {server.Owner.Username}",
            iconUrl: (string.IsNullOrEmpty(server.Owner.AvatarHash) ? null : server.Owner.AvatarUrl) ?? string.Empty);
        var channelString = new StringBuilder();

        #region channel list string builder

        foreach (var channel in server.Channels)
        {
            switch (channel.Value.Type)
            {
                case ChannelType.Text:
                    channelString.Append($"[`#{channel.Value.Name} (💬)`]");
                    break;
                case ChannelType.Voice:
                    channelString.Append($"`[{channel.Value.Name} (🔈)]`");
                    break;
                case ChannelType.Category:
                    channelString.Append($"`[{channel.Value.Name.ToUpper()} (📁)]`");
                    break;
                case ChannelType.News:
                    channelString.Append($"`[{channel.Value.Name.ToUpper()} (🗞)]`");
                    break;
                case ChannelType.Private:
                    channelString.Append($"`[{channel.Value.Name.ToUpper()} (🤫)]`");
                    break;
                default:
                    channelString.Append($"`[{channel.Value.Name} (❓)]`");
                    break;
            }
        }

        #endregion

        embed.AddField("Channels", channelString.ToString());

        var roleString = new StringBuilder();

        #region role list string builder

        foreach (var role in server.Roles)
        {
            roleString.Append($"[`{role.Value.Name}`] ");
        }

        #endregion

        embed.AddField("Roles", roleString.ToString());
        
        embed.AddField(
            "Misc", 
            $"Large: {(server.IsLarge ? "yes" : "no")}.\n" +
            $"Default Notifications: {server.DefaultMessageNotifications}.\n" +
            $"Explicit content filter: {server.ExplicitContentFilter}.\n" +
            $"MFA Level: {server.MfaLevel}.\n" +
            $"Verification Level: {server.VerificationLevel}");

        embed.WithThumbnail(server.GetIconUrl(ImageFormat.Auto));

        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("channel", "Information about a channel")]
    public async Task ChannelAsync(InteractionContext ctx,
        [Option("channel", "Channel to list information about.")] DiscordChannel channel)
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle($"#{channel.Name} ID: ({channel.Id})")
            .WithDescription($"Topic: {channel.Topic}\nCreated at: <t:{channel.CreationTimestamp.ToUnixTimeSeconds()}:F>" +
                             $"{(channel.ParentId != null ? $"\nChild of `{channel.Parent.Name.ToUpper()}` ID: ({channel.Parent.Id})" : "")}");

        if (channel.IsCategory)
        {
            var channelString = new StringBuilder();

            #region channel list string builder

            foreach (var childChannel in channel.Children)
            {
                switch (childChannel.Type)
                {
                    case ChannelType.Text:
                        channelString.Append($"[`#{childChannel.Name} (💬)`]");
                        break;
                    case ChannelType.Voice:
                        channelString.Append($"`[{childChannel.Name} (🔈)]`");
                        break;
                    case ChannelType.Category:
                        channelString.Append($"`[{childChannel.Name.ToUpper()} (📁)]`");
                        break;
                    case ChannelType.News:
                        channelString.Append($"`[{childChannel.Name.ToUpper()} (🗞)]`");
                        break;
                    case ChannelType.Private:
                        channelString.Append($"`[{childChannel.Name.ToUpper()} (🤫)]`");
                        break;
                    default:
                        channelString.Append($"`[{childChannel.Name} (❓)]`");
                        break;
                }
            }

            #endregion
            embed.AddField("Children of category", channelString.ToString());
        }

        if (channel.Type == ChannelType.Voice)
        {
            embed.AddField("Voice", $"Bit rate: {channel.Bitrate}\nUser limit: {(channel.UserLimit == 0 ? "Unlimited" : $"{channel.UserLimit}")}");

        }
        embed.AddField(
            "Misc", 
            $"NSFW: {(channel.IsNSFW ? "Yes 😏" : "No")}\n" + 
            $"{(channel.Type == ChannelType.Text ? $"Last message ID: {(await channel.GetMessagesAsync(1))[0].Id}" : "")}");
        await ctx.CreateResponseAsync(embed);
    }
    
}