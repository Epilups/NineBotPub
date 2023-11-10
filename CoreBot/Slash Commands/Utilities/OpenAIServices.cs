using System.Transactions;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using CoreBot.External_Classes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI_API;
using OpenAI_API.Images;
using OpenAI_API.Models;

namespace CoreBot.Slash_Commands.Utilities;

public class OpenAIServices : ApplicationCommandModule
{
    [SlashCommand("davinci", "Generate text using OpenAI's Davinci model.")]
    [SlashCooldown(1, 120, SlashCooldownBucketType.User)]

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
    [SlashCooldown(1, 120, SlashCooldownBucketType.User)]

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
        chat.Model = "gpt-4-1106-preview";
        
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
    [SlashCooldown(1, 120, SlashCooldownBucketType.User)]
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
    
    
    [SlashCommand("conversation", "Converse with GPT4-Turbo")]
    [SlashCooldown(5, 30, SlashCooldownBucketType.User)]

    public async Task Conversation(InteractionContext ctx,
        [Option("message", "The message you want to send to the bot")]
        string prompt)
    {
        await ctx.DeferAsync();

        var apiKey = Configuration.PaulaKey;
        var openai = new OpenAIAPI(apiKey);

        var database = new DBSetup(Configuration.MongoConnectionString, "MessageContext").GetDatabase();
        var collection = database.GetCollection<BsonDocument>("UserAIContext");

        var filter = Builders<BsonDocument>.Filter.Eq("UserId", ctx.User.Id.ToString());
        var userContext = collection.Find(filter).FirstOrDefault();

        string context = string.Empty;

        if (userContext != null)
        {
            // If the user has a chat history, retrieve it.
            context = userContext["MessageContext"].AsString;
        }

        // Append the current user's message to the context.
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            context += Environment.NewLine + prompt;
        }

        var chat = openai.Chat.CreateConversation();
        chat.Model = "gpt-4-1106-preview";
        chat.AppendUserInput(context);
        var response = chat.GetResponseFromChatbotAsync();
            
        // Store the updated context back into the database.

        if (userContext == null)
        {
            
            userContext = new BsonDocument
            {
                {"UserId", ctx.User.Id.ToString()},
                {"MessageContext", "Current Conversation: " + context + "\n" + "Bot: "+ response.Result},
            };
            collection.InsertOne(userContext);
        }
        else
        {
            userContext["MessageContext"] = "\n" + context + "\n" + "Bot: "+ response.Result;
            
            collection.ReplaceOne(filter, userContext);
        }

        var embed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Azure,
            Title = prompt,
            Description = response.Result
        };
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

}
