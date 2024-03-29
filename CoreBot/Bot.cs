﻿using System.Collections.Concurrent;
using CoreBot.Slash_Commands.Game_Commands;
using CoreBot.Slash_Commands.Random_commands;
using CoreBot.Slash_Commands.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;

namespace CoreBot;

public class Bot
{
    
    public DiscordClient? Client { get; private set; }
    public CommandsNextExtension? Commands {get; private set; }
    
    public async Task RunAsync()
    {
        var config = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            Token = Configuration.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };
        Client = new DiscordClient(config);
        Client.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2)
        });
        
        //events
        Client.Ready += OnClientReady;
        //Client.MessageDeleted += OnMessageDeleted;
        //Client.MessageCreated += OnMessageCreated;
        
        
        var commandsConfig = new CommandsNextConfiguration
        {
            StringPrefixes = new[] { Configuration.Prefix },
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };

        Commands = Client.UseCommandsNext(commandsConfig);
        var slashCommandsConfig = Client.UseSlashCommands();
        
        //slash commands

        slashCommandsConfig.RegisterCommands<Avatar>();
        slashCommandsConfig.RegisterCommands<InspiroBot>();
        slashCommandsConfig.RegisterCommands<OpenAIServices>();
        slashCommandsConfig.RegisterCommands<UrbanDefinition>();
        slashCommandsConfig.RegisterCommands<GoodreadsQuote>();
        slashCommandsConfig.RegisterCommands<ImageSearch>();
        slashCommandsConfig.RegisterCommands<PolarBear>();
        slashCommandsConfig.RegisterCommands<Trivia>();
        slashCommandsConfig.RegisterCommands<BlackTeas>();
        slashCommandsConfig.RegisterCommands<Status>();
        slashCommandsConfig.RegisterCommands<Coinflip>();
        slashCommandsConfig.RegisterCommands<Information>();
        slashCommandsConfig.RegisterCommands<Moderation>();
        slashCommandsConfig.RegisterCommands<Purges>();
        slashCommandsConfig.RegisterCommands<StealEmojiMessage>();
        slashCommandsConfig.RegisterCommands<Translation>();

        slashCommandsConfig.SlashCommandErrored += OnSlashCommandError;
        
        
        await Client.ConnectAsync();
        await Task.Delay(-1);

    }
    
    //methods for handling arising events

    /*
    private Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        var database = new DBSetup(Configuration.MongoConnectionString, "MessageStorage").GetDatabase();
        var collection = database.GetCollection<BsonDocument>("Messages");
        var document = new BsonDocument
        {
            { "user", e.Author.Username },
            { "userId", e.Author.Id.ToString() },
            { "content", e.Message.Content },
            { "contentId", e.Message.Id.ToString() },
            { "guild", e.Guild.Name },
            { "channel", e.Channel.Name },
            { "channelId", e.Channel.Id.ToString() },
            { "created", e.Message.CreationTimestamp.DateTime },
            { "isDeleted", false }

        };
        
        collection.InsertOne(document);
        return Task.CompletedTask;
    }

    private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        var database = new DBSetup(Configuration.MongoConnectionString, "MessageStorage").GetDatabase();
        var collection = database.GetCollection<BsonDocument>("Messages");
    
        var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("userId", e.Message.Author.Id.ToString()),
            Builders<BsonDocument>.Filter.Eq("contentId", e.Message.Id.ToString())
        );
        

        var update = Builders<BsonDocument>.Update.Set("isDeleted", true);
    
        collection.UpdateOne(filter, update);
        
        return Task.CompletedTask;
    }
    */


    private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        return Task.CompletedTask;
    }

    private async Task OnSlashCommandError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        await e.Context.DeferAsync(true);
        
        if (e.Exception is SlashExecutionChecksFailedException)
        {
            var castedException = (SlashExecutionChecksFailedException)e.Exception;
            string cooldownTimer = string.Empty;

            foreach (var check in castedException.FailedChecks)
            {
                var cooldown = (SlashCooldownAttribute)check;
                TimeSpan timeLeft = cooldown.GetRemainingCooldown(e.Context);
                cooldownTimer = timeLeft.ToString(@"hh\:mm\:ss");
            }

            var cooldownMessage = new DiscordEmbedBuilder()
                .WithTitle($"Wait for the cooldown to end {e.Context.User.Username}")
                .WithColor(DiscordColor.Red)
                .WithDescription(cooldownTimer);
            
            await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(cooldownMessage));
        }
    }
}