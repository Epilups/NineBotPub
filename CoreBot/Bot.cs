using CoreBot.External_Classes;
using CoreBot.Slash_Commands;
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

namespace CoreBot;

public class Bot : ApplicationCommandModule
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
        
        Client.Ready += OnClientReady;

        Client.ComponentInteractionCreated += ButtonPressResponseTest;
        
        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new [] {Configuration.Prefix},
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };
        
        Commands = Client.UseCommandsNext(commandsConfig);
        var slashCommandsConfig = Client.UseSlashCommands();
        
        //prefix based commands
        
        
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
        
        
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }
    
    private async Task ButtonPressResponseTest(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        DiscordButtonComponent button1 = new DiscordButtonComponent(ButtonStyle.Primary, "1", "Button 1");
        DiscordButtonComponent button2 = new DiscordButtonComponent(ButtonStyle.Primary, "2", "Button 2");

        var embed1 = new DiscordEmbedBuilder()
            .WithTitle("Test 1")
            .WithColor(DiscordColor.Azure)
            .WithDescription("This is a test message to see if button 1 works");
        
        var embed2 = new DiscordEmbedBuilder()
            .WithTitle("Test 2")
            .WithColor(DiscordColor.Azure)
            .WithDescription("This is a test message to see if button 2 works");
        
        if (e.Interaction.Data.CustomId == "1")
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().AddEmbed(embed1)
                    .AddComponents(button1, button2));
        }
        else if (e.Interaction.Data.CustomId == "2")
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().AddEmbed(embed2)
                    .AddComponents(button1, button2));
        }
    }

    private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        return Task.CompletedTask;
    }
}