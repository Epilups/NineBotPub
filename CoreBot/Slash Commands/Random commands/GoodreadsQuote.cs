using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HtmlAgilityPack;

namespace CoreBot.Slash_Commands.Random_commands;

public class GoodreadsQuote : ApplicationCommandModule
    {

        private readonly string _baseUrl = "https://www.goodreads.com/quotes?page=";
        private readonly Random _random = new();

        [SlashCommand("quote", "Generates a random quote.")]
        public async Task Quote(InteractionContext ctx)
        {
            List<string> quotes = await GetQuotes();
            
            if (quotes.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Failed to retrieve any quotes."));
                return;
            }

            int index = _random.Next(quotes.Count);
            string quote = quotes[index];
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Here's a quote!")
                .WithDescription(quote)
                .WithColor(DiscordColor.Azure);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }

        private async Task<List<string>> GetQuotes()
        {
            List<string> quotes = new List<string>();

            int pageNumber = _random.Next(1, 101); // generate a random page number
            string url = _baseUrl + pageNumber + "&ref=nav_comm_quotes"; // use the random page number in the URL

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string html = await response.Content.ReadAsStringAsync();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var quoteNodes = document.DocumentNode.SelectNodes("//div[@class='quoteText']");

            foreach (var node in quoteNodes)
            {
                string text = node.InnerHtml.Trim();

                //removes <br> tags from the text
                var brNodes = node.SelectNodes("//br");
                if (brNodes != null)
                {
                    foreach (var brNode in brNodes)
                    {
                        brNode.Remove();
                    }
                }
                
                var italicNodes = node.SelectNodes("//i");
                if (italicNodes != null)
                {
                    foreach (var italicNode in italicNodes)
                    {
                        italicNode.Remove();
                    }
                }

                int startIndex = text.IndexOf("&ldquo;", StringComparison.Ordinal) + 7;
                int endIndex = text.IndexOf("&rdquo;", startIndex, StringComparison.Ordinal);
                string quote = text.Substring(startIndex, endIndex - startIndex);

                var authorNode = node.SelectSingleNode(".//span[@class='authorOrTitle']");
                string author = authorNode.InnerText.Trim().Replace(",", "");

                quotes.Add($"{quote} \n\n- {author}");
            }

            return quotes;
        }
    }