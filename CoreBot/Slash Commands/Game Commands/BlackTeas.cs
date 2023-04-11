using System.Collections;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace CoreBot.Slash_Commands.Game_Commands;

public class BlackTeas : ApplicationCommandModule
    {
        private readonly string[] _words;
        
        public BlackTeas()
        {
            _words = Configuration.words;
        }
        
        [SlashCommand("blacktea", "Play a game of black tea")]
        public async Task Game(InteractionContext ctx,
            [Option("rounds", "The number of rounds to play")]
            long rounds)
        {
            await ctx.DeferAsync();

            var interactivity = ctx.Client.GetInteractivity();
            
            int points = 0;
            for (int i = 0; i < rounds; i++)
            {
                string sequence = GenerateRandomSequence();
                bool validSequence = false;
                while (!validSequence)
                {
                    if (CheckForWords(sequence))
                    {
                        validSequence = true;
                    }
                    else
                    {
                        sequence = GenerateRandomSequence();
                    }
                }
                
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Enter a word that contains the character sequence: **{sequence}**",
                    Color = DiscordColor.Azure
                };
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Game starting..."));
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed));
                
                var messageResult = await interactivity.WaitForMessageAsync(
                    x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
                
                string userInput = messageResult.Result.Content.ToLower();

                if (userInput.Contains(sequence))
                {
                    if (IsValidWord(userInput))
                    {
                        var correctEmbed = new DiscordEmbedBuilder
                        {
                            Title = $"Correct! The word {userInput} is a valid answer",
                            Color = DiscordColor.Green
                        };
                        await ctx.Channel.SendMessageAsync(embed: correctEmbed);
                        points++;
                    }
                    else
                    {
                        var invalidEmbed = new DiscordEmbedBuilder
                        {
                            Title = $"Sorry, {userInput} is not a valid answer.",
                            Color = DiscordColor.Red
                        };
                        await ctx.Channel.SendMessageAsync(embed: invalidEmbed);
                    }
                }
                else
                {
                    var invalidEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Sorry, your word must contain the sequence {sequence}.",
                        Color = DiscordColor.Red
                    };
                    await ctx.Channel.SendMessageAsync(embed: invalidEmbed);
                }
            }
            var finalEmbed = new DiscordEmbedBuilder
            {
                Title = $"You scored {points} points out of {rounds}.",
                Color = DiscordColor.Azure
            };
            await ctx.Channel.SendMessageAsync(embed: finalEmbed);
        }
        
        private string GenerateRandomSequence()
        {
            Random random = new Random();
            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            string sequence = "";
            for (int i = 0; i < 3; i++)
            {
                sequence += alphabet[random.Next(alphabet.Length)];
            }

            return sequence;
        }
        private bool CheckForWords(string sequence)
        {
            foreach (string w in _words)
            {
                if (w.Contains(sequence))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsValidWord(string word)
        {
            return ((IList)_words).Contains(word);
        }
    }