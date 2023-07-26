using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreBot.Slash_Commands.Utilities;

public class Translation : ApplicationCommandModule
{

    [SlashCommand("translate", "Translates a prompt to a target language")]
    public async Task Translate(InteractionContext ctx,
        [Option("prompt", "The message to be translated")] string prompt,
        [Option("from", "Language to translate from")] string from,
        [Option("to", "The target language")] string to)
    
    {
        await ctx.DeferAsync();
        string route = $"/translate?api-version=3.0&from={from}&to={to}";
        string textToTranslate = prompt;
        object[] body = { new { Text = textToTranslate } };
        var requestBody = JsonConvert.SerializeObject(body);
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            //building the request
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Configuration.TranslationEndpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", Configuration.TranslationKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", Configuration.TranslationLocation);
            
            //sending the request and get response
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            string result = await response.Content.ReadAsStringAsync();

            string json = result;
            JArray jsonArray = JArray.Parse(json);
            string textResult = (string)jsonArray[0]["translations"][0]["text"];
            string toResult = (string)jsonArray[0]["translations"][0]["to"];

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.Member.Username}: {prompt}")
                .WithColor(DiscordColor.Azure)
                .WithDescription("## " + textResult + $"\n\nTranslated from **{from}** to **{toResult}**");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

        }
    }
    
}