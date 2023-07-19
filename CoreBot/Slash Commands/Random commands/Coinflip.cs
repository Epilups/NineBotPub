using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Random_commands;

public class Coinflip : ApplicationCommandModule
{
    [SlashCommand("coinflip", "Flips a coin")]
    public async Task FlipCoin(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Alright! flipping a coin for you...");
        await Task.Delay(1000);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("ㅤ\n✊🪙"));
        await Task.Delay(200);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("ㅤㅤ🪙ㅤㅤ\n☝️✨"));
        await Task.Delay(200);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("ㅤㅤ✨🪙ㅤ\n☝️✨"));
        await Task.Delay(200);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("ㅤㅤ✨✨\n☝️✨ㅤㅤ🪙"));
        await Task.Delay(1000);
        var rng = new Random().Next(0, 2);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"🤔 Hmm... Looks like it landed on {(rng == 0? "**heads**" : "**tails**")}!"));

    }
}