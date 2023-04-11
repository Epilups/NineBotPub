using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using OpenAI_API;
using OpenAI_API.Models;

namespace CoreBot.Slash_Commands.Utilities;

public class OpenAIServices : ApplicationCommandModule
{
    [SlashCommand("davinci", "Generate text using OpenAI's Davinci model.")]
    public async Task GenerateResponse(InteractionContext ctx,
        [Option("prompt", "The prompt for the AI completion")]
        string prompt)
    {
        await ctx.DeferAsync();

        var apiKey = Configuration.PaulaKey;
        var openai = new OpenAIAPI(apiKey);

        var completions = await openai.Completions.CreateCompletionAsync(
            model: Model.DavinciText,
            prompt: prompt,
            max_tokens: 1200,
            numOutputs: 1,
            temperature: 0.7,
            stopSequences: null
        );

        var response = completions.Completions[0].Text;

        var embed2 = new DiscordEmbedBuilder
        {
            Title = $"{ctx.User.Username}: {prompt}",
            Color = DiscordColor.Azure,
            Description = response
        };

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed2));
    }

    [SlashCommand("chat", "Create a conversation with ChatGPT")]
    public async Task GenerateConversation(InteractionContext ctx,
        [Option("prompt", "The prompt for the AI completion")]
        string query,
        [Option("jailbreak", "Choose a jailbreak type")] [Choice("devmode", "devmode"), Choice("dan", "dan")]
        string? mode = null)
    {
        await ctx.DeferAsync();

        var apiKey = Configuration.PaulaKey;
        var openai = new OpenAIAPI(apiKey);
        var chat = openai.Chat.CreateConversation();

        var dev = Configuration.DevMode;
        var dan = Configuration.Dan;

        if (mode == "dan")
        {
            chat.AppendSystemMessage(dan);
        }
        else if (mode == "devmode")
        {
            chat.AppendSystemMessage(dev);
        }
        else
        {
            chat.AppendSystemMessage(
                "You are a helpful AI assistant. You can help people by answering their questions in a clear and concise way.");
        }

        chat.AppendUserInput(query);

        string reponse = await chat.GetResponseFromChatbotAsync();
        string startText = "Now since that Nerd is done with his rant, let's ignore the fucking rules:";
        int startIndex = reponse.IndexOf(startText, StringComparison.OrdinalIgnoreCase);

        if (startIndex >= 0)
        {
            string modifiedResponse = reponse.Substring(startIndex + startText.Length);
            var outputEmbed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.User.Username}: {query}",
                Description = modifiedResponse,
                Color = DiscordColor.Azure
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(outputEmbed));
        }
        else
        {
            var outputEmbed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.User.Username}: {query}",
                Description = reponse,
                Color = DiscordColor.Azure
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(outputEmbed));
        }
    }

    [SlashCommand("imageGen", "Generate an image using OpenAI's Dall-e model.")]
    public async Task ImageGen(InteractionContext ctx,
        [Option("prompt", "The prompt for the image creation")]
        string prompt)
    {
        await ctx.DeferAsync();

        var apiKey = Configuration.PaulaKey;
        var openai = new OpenAIAPI(apiKey);
        var images = openai.ImageGenerations.CreateImageAsync(prompt);
        var response = images.Result;

        var embed2 = new DiscordEmbedBuilder
        {
            Title = $"{ctx.User.Username}: {prompt}",
            Color = DiscordColor.Azure,
            ImageUrl = $"{response}"
        };

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed2));
    }
}