using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands;

public class TestingComponents : ApplicationCommandModule
{
    [SlashCommand("button", "Test")]
    public async Task ButtonExample(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        
        DiscordButtonComponent button1 = new DiscordButtonComponent(ButtonStyle.Primary, "1", "Button 1");
        DiscordButtonComponent button2 = new DiscordButtonComponent(ButtonStyle.Primary, "2", "Button 2");

        var message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Azure)
                .WithTitle("this is a message with buttons")
                .WithDescription("please select button")
            )
            .AddComponents(button1)
            .AddComponents(button2);
        await ctx.Channel.SendMessageAsync(message);
    }
}