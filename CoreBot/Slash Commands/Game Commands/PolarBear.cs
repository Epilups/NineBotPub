using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Game_Commands;

public class PolarBear : ApplicationCommandModule
    {
        [SlashCommand("polarbear", "Play a game of polar bears")]
        public async Task PolarBearGame(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var interactivity = ctx.Client.GetInteractivity();

            DiscordEmoji[] optionEmojis = 
            {
                DiscordEmoji.FromName(ctx.Client, ":diceone:"),
                DiscordEmoji.FromName(ctx.Client, ":dicetwo:"),
                DiscordEmoji.FromName(ctx.Client, ":dicethree:"),
                DiscordEmoji.FromName(ctx.Client, ":dicefour:"),
                DiscordEmoji.FromName(ctx.Client, ":dicefive:"),
                DiscordEmoji.FromName(ctx.Client, ":dicesix:")
            };
            
            int[] arr = new int[6];
            Random rand = new Random();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = rand.Next(1, 7);
            }

            var bears = 0;
            
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 3)
                {
                    bears += 2;
                }
                else if (arr[i] == 5)
                {
                    bears += 4;
                }
            }

            var polarBearMessage = new DiscordEmbedBuilder()
                .WithTitle("Polar bears sit around holes in the ice like petals on a flower")
                .WithDescription("How many polar bears are there?" + "\n" + string.Join(" ", arr.Select(x => optionEmojis[x - 1])))
                .WithColor(DiscordColor.Azure);

            var fishMessage = new DiscordEmbedBuilder()
                .WithTitle("You got it correct. Now with the same dice, tell me how many fish there are")
                .WithDescription("How many fish are there?" + "\n" + string.Join(" ", arr.Select(x => optionEmojis[x - 1])))
                .WithColor(DiscordColor.Green);

            var planktonMessage = new DiscordEmbedBuilder()
                .WithTitle("You got it correct. Now with the same dice, tell me how many plankton there are")
                .WithDescription("Fish eat plankton\nHow many plankton are there?" + "\n" + string.Join(" ", arr.Select(x => optionEmojis[x - 1])))
                .WithColor(DiscordColor.Green);
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Game starting..."));

            await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(polarBearMessage));
            
            var message1 = await interactivity.WaitForMessageAsync(
                x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
            
            var fish = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 5)
                {
                    fish += 2;
                }
                else if (arr[i] == 3)
                {
                    fish += 4;
                }
                else if (arr[i] == 1)
                {
                    fish += 6;
                }
            }

            var plankton = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 2)
                {
                    plankton += 2*7+5;
                }
                else if(arr[i] == 4)
                {
                    plankton += 4*7+3;
                }
                else if(arr[i] == 6)
                {
                    plankton += 6*7+1;
                }
            }
            var losingMessage = new DiscordEmbedBuilder
            {
                Title = "Sorry, you lost",
                Color = DiscordColor.Red
            };
            
            if (message1.Result.Content == bears.ToString())
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(fishMessage));
                
                var message2 = await interactivity.WaitForMessageAsync(
                    x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
                
                if (message2.Result.Content == fish.ToString())
                {
                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(planktonMessage));
                    
                    var message3 = await interactivity.WaitForMessageAsync(
                        x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
                    
                    
                    if (message3.Result.Content == plankton.ToString())
                    {
                        var winnerMessage = new DiscordEmbedBuilder
                        {
                            Title = "Congratulations, you are a true polar bear chad!",
                            Color = DiscordColor.Green
                        };
                        
                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(winnerMessage));
                    }
                    
                    else
                    {
                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(losingMessage.WithDescription($"Sorry, the correct answer was {plankton}.")));
                    }
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(losingMessage.WithDescription($"Sorry, the correct answer was {fish}.")));
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(losingMessage.WithDescription($"Sorry, the correct answer was {bears}.")));
            }
        }
    }