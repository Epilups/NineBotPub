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
        
        var json = string.Empty;
        var projectFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        Console.WriteLine(projectFolder);
        var file = Path.Combine(projectFolder, "CoreBot/configuration.json");
        await using (var fs = File.OpenRead(file))
        using (var sr = new StreamReader(fs, new UTF8Encoding(false))) json = await sr.ReadToEndAsync();

        var configJson = JsonConvert.DeserializeObject<ConfigurationJSON>(json);
        
        
        var config = new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = configJson.Token,
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
            StringPrefixes = new [] {configJson.Prefix},
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