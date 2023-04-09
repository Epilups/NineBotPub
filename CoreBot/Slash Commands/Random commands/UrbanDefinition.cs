using System.Text.RegularExpressions;
using CoreBot.External_Classes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace CoreBot.Slash_Commands.Random_commands;

public class UrbanDefinition : ApplicationCommandModule
    {
        private const int ResultsPerPage = 1;
        private long _page = 1;
        [SlashCommand("urban", "Search for a word on Urban Dictionary.")]
        public async Task DefineCommand(InteractionContext ctx,
            [Option("query", "The search query for the results.")] string query)
        {
            var encodedTerm = Uri.EscapeDataString(query);
            using (var http = new HttpClient())
            {
                var response = await http.GetAsync($"https://api.urbandictionary.com/v0/define?term={encodedTerm}");
                
                response.EnsureSuccessStatusCode();
                
                var result = JsonConvert.DeserializeObject<UrbanDictionaryResult>(await response.Content.ReadAsStringAsync());
                
                var entry = result.List[0];

                await ctx.DeferAsync();
                
                var startIndex = (_page - 1) * ResultsPerPage;
                
                var definition = entry.Definition;
                if (definition != null)
                {
                    definition = ReplaceBracketsWithHyperlinks(definition);

                    var example = entry.Example;
                    if (example != null)
                    {
                        example = ReplaceBracketsWithHyperlinks(example);

                        var likes = entry.ThumbsUp;
                        var dislikes = entry.ThumbsDown;

                        var embed = new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.Azure)
                            .WithTitle($"Urban dictionary results for '{query}'")
                            .WithDescription(
                                $"[View in website](https://www.urbandictionary.com/define.php?term={Uri.EscapeDataString(query)})")
                            .AddField("Definition", definition)
                            .AddField("Example", example)
                            .AddField("Rating",
                                $"{likes} {DiscordEmoji.FromName(ctx.Client, ":uparrow1:")} {dislikes} {DiscordEmoji.FromName(ctx.Client, ":downarrow1:")}");

                        var message = new DiscordMessageBuilder().AddEmbed(
                            embed.WithFooter(
                                $"Page {_page}/{(int)Math.Ceiling((double)result.List.Count / ResultsPerPage)}"));

                        var responseBuilder = await ctx.EditResponseAsync(new DiscordWebhookBuilder(message));

                        await responseBuilder.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_left:"));
                        await responseBuilder.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_right:"));

                        var interactivity = ctx.Client.GetInteractivity();

                        while (true)
                        {
                            var reactionResult = await interactivity.WaitForReactionAsync(x
                                => x.Message == responseBuilder && x.User.Id == ctx.User.Id &&
                                   (x.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_left:") ||
                                    x.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

                            if (reactionResult.Result.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_left:"))
                            {
                                _page = Math.Max(1, _page - 1);
                                startIndex = (_page - 1) * ResultsPerPage;
                            }
                            else if (reactionResult.Result.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_right:"))
                            {
                                _page = Math.Min((int)Math.Ceiling((double)result.List.Count / ResultsPerPage),
                                    _page + 1);
                                startIndex = (_page - 1) * ResultsPerPage;
                            }


                            entry = result.List[(int)startIndex];
                            definition = entry.Definition;
                            if (definition != null)
                            {
                                definition = ReplaceBracketsWithHyperlinks(definition);

                                example = entry.Example;
                                if (example != null)
                                {
                                    example = ReplaceBracketsWithHyperlinks(example);

                                    likes = entry.ThumbsUp;
                                    dislikes = entry.ThumbsDown;

                                    embed = new DiscordEmbedBuilder()
                                        .WithColor(DiscordColor.Azure)
                                        .WithTitle($"UrbanDictionary results for '{query}'")
                                        .WithDescription(
                                            $"[View in website](https://www.urbandictionary.com/define.php?term={Uri.EscapeDataString(query)})")
                                        .AddField("Definition", definition)
                                        .AddField("Example", example)
                                        .AddField("Rating",
                                            $"{likes} {DiscordEmoji.FromName(ctx.Client, ":uparrow1:")} {dislikes} {DiscordEmoji.FromName(ctx.Client, ":downarrow1:")}");
                                }
                            }

                            if (result.List.Count > ResultsPerPage)
                            {
                                embed.WithFooter(
                                    $"Page {_page}/{(int)Math.Ceiling((double)result.List.Count / ResultsPerPage)}");
                            }

                            message = new DiscordMessageBuilder().AddEmbed(embed);

                            await responseBuilder.ModifyAsync(message);
                            await responseBuilder.DeleteReactionAsync(reactionResult.Result.Emoji,
                                reactionResult.Result.User);
                        }
                    }
                }
            }
        }

        private static string ReplaceBracketsWithHyperlinks(string text)
        {
            var pattern = @"\[(.*?)\]";
            var regex = new Regex(pattern);

            var replacedText = regex.Replace(text, match =>
            {
                var word = match.Groups[1].Value;
                return $"[{word}](https://www.urbandictionary.com/define.php?term={Uri.EscapeDataString(word)})";
            });
            return replacedText;
        }
    }