using System.Text;
using CoreBot.Slash_Commands.Game_Commands;
using CoreBot.Slash_Commands.Random_commands;
using CoreBot.Slash_Commands.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace CoreBot;

public class Bot
{
    public DiscordClient? Client { get; private set; }
    public CommandsNextExtension? Commands {get; private set; }

    public async Task RunAsync()
    {
        //var json = string.Empty;
        //await using (var fs = File.OpenRead("configuration.json"))
        //using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            //json = await sr.ReadToEndAsync();

        //var configJson = JsonConvert.DeserializeObject<ConfigurationJSON>(json);
        
        var config = new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = "MTA4NDQ2NDY4Mjk1MTI1MDAxMQ.GxumsV.QsHkK9QNm9DRy3i9dJM1oCtK8ZvSFlpl05b4EU",
            TokenType = TokenType.Bot,
            AutoReconnect = true
        };
        
        Client = new DiscordClient(config);
        Client.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2)
        });
        
        Client.Ready += OnClientReady;
        
        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new [] { ">" },
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
    
    private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        return Task.CompletedTask;
    }
}