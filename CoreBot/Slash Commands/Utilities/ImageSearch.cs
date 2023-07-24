using CoreBot.External_Classes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace CoreBot.Slash_Commands.Utilities;

public class ImageSearch : ApplicationCommandModule
{
    private const int ResultsPerPage = 1;

    [SlashCommand("image", "Search for images.")]
    [SlashCooldown(1, 120, SlashCooldownBucketType.User)]

    public async Task SearchImages(InteractionContext ctx,
        [Option("query", "The search query.")] string query,
        [Option("page", "The page number.")] long page = 1)
    {
        await ctx.DeferAsync();

        var results = new ImageSearchResults(query);

        var startIndex = (page - 1) * ResultsPerPage;

        var embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithTitle($"Search Results for '{query}'")
            .WithImageUrl(results[startIndex].Url);

        embedBuilder.WithDescription(results[startIndex].Title);

        if (results.Count > ResultsPerPage)
        {
            embedBuilder.WithFooter($"Page {page}/{(int)Math.Ceiling((double)results.Count / ResultsPerPage)}");
        }

        var message = new DiscordMessageBuilder().AddEmbed(embedBuilder);
        var response = await ctx.EditResponseAsync(new DiscordWebhookBuilder(message));

        if (results.Count > ResultsPerPage)
        {
            await response.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_left:"));
            await response.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_right:"));

            var interactivity = ctx.Client.GetInteractivity();

            while (true)
            {
                var reactionResult = await interactivity.WaitForReactionAsync(
                    x => x.Message == response &&
                         x.User.Id == ctx.User.Id &&
                         (x.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_left:") ||
                          x.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                if (reactionResult.Result.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_left:"))
                {
                    page = Math.Max(1, page - 1);
                }
                else if (reactionResult.Result.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_right:"))
                {
                    page = Math.Min((int)Math.Ceiling((double)results.Count / ResultsPerPage), page + 1);
                }

                startIndex = (page - 1) * ResultsPerPage;

                embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle($"Search Results for '{query}'")
                    .WithImageUrl(results[startIndex].Url);

                embedBuilder.WithDescription(results[startIndex].Title);

                if (results.Count > ResultsPerPage)
                {
                    embedBuilder.WithFooter(
                        $"Page {page}/{(int)Math.Ceiling((double)results.Count / ResultsPerPage)} of Google Image search results.");
                }

                message = new DiscordMessageBuilder().AddEmbed(embedBuilder);
                await response.ModifyAsync(message);
                await response.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
            }
        }
    }
}