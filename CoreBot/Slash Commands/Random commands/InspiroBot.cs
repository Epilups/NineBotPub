using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CoreBot.Slash_Commands.Random_commands;

public class InspiroBot : ApplicationCommandModule
{
    private readonly HttpClient _httpClient = new ();
    [SlashCommand("inspire", "Generates an inspirational image from Inspirobot")]
    [SlashCooldown(1, 120, SlashCooldownBucketType.User)]
    public async Task Inspire(InteractionContext ctx)
    {
        await ctx.DeferAsync();
            
        var response = await _httpClient.GetAsync("https://inspirobot.me/api?generate=true");
        var imageUrl = await response.Content.ReadAsStringAsync();
            
        //custom color

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Here's your inspiration")
            .WithImageUrl(imageUrl)
            .WithColor(DiscordColor.Azure)
            .Build();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        
    }
    
}