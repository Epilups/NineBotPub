using CoreBot.External_Classes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace CoreBot.Slash_Commands.Game_Commands;

public class Trivia : ApplicationCommandModule
{
    private readonly string _baseUrl = "https://the-trivia-api.com/api/questions?";

    [SlashCommand("trivia", "Creates a game of trivia")]
    public async Task TriviaGame(InteractionContext ctx,

        [Option("category", "The category of trivia to play, defaults to random for each question")]
        [Choice("General Knowledge", "general_knowledge")]
        [Choice("Arts & Literature", "arts_and_literature")]
        [Choice("Geography", "geography")]
        [Choice("History", "history")]
        [Choice("Science", "science")]
        [Choice("Sport & Leisure", "sport_and_leisure")]
        [Choice("Film & TV", "film_and_tv")]
        [Choice("Music", "music")]
        [Choice("Society & Culture", "society_and_culture")]
        [Choice("Food & Drink", "food_and_drink")]
        string? category = null,

        [Option("difficulty", "The difficulty of the trivia questions, defaults to random for each question")]
        [Choice("Easy", "easy")]
        [Choice("Medium", "medium")]
        [Choice("Hard", "hard")]
        string? difficulty = null,

        [Option("rounds", "The number of rounds to play, defaults to 5")]
        string? rounds = null)
    {
        await ctx.DeferAsync();
        var interactivity = ctx.Client.GetInteractivity();

        int numRounds = 5;
        if (!string.IsNullOrWhiteSpace(rounds))
        {
            numRounds = int.Parse(rounds);
        }

        var url = _baseUrl + "limit=" + numRounds;
        if (category != null)
        {
            url += "&categories=" + category;
        }

        if (difficulty != null)
        {
            url += "&difficulty=" + difficulty;
        }

        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var triviaQuestions = JsonConvert.DeserializeObject<List<TriviaResults>>(responseString);

        int score = 0;

        if (triviaQuestions != null)
            foreach (var q in triviaQuestions)
            {
                if (q is { CorrectAnswer: { }, IncorrectAnswers: { } })
                {
                    var answers = new List<string>
                        { q.CorrectAnswer, q.IncorrectAnswers[0], q.IncorrectAnswers[1], q.IncorrectAnswers[2] };
                    var random = new Random();
                    answers = answers.OrderBy(x => random.Next()).ToList();
                    var correctAnswerIndex = answers.IndexOf(q.CorrectAnswer);


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Game starting..."));
                    if (q is { Question: { }, Difficulty: { } })
                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                            new DiscordEmbedBuilder()
                                .WithTitle("Question")
                                .WithDescription(q.Question)
                                .AddField("Category", $"{q.Category}", true)
                                .AddField("Difficulty", q.Difficulty, true)
                                .AddField("Options",
                                    $"1. {answers[0]}\n2. {answers[1]}\n3. {answers[2]}\n4. {answers[3]}")
                                .WithColor(DiscordColor.Azure)
                                .WithFooter("Type the index of the answer in chat to answer the question.")));

                    var answer = await interactivity.WaitForMessageAsync(
                        x => x.ChannelId == ctx.Channel.Id && x.Author.Id == ctx.User.Id).ConfigureAwait(false);
                    if (answer.Result.Content == (correctAnswerIndex + 1).ToString())
                    {
                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                            new DiscordEmbedBuilder()
                                .WithDescription("Correct!")
                                .WithColor(DiscordColor.Green)));
                        score++;
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                            new DiscordEmbedBuilder()
                                .WithDescription(
                                    $"Incorrect! The correct answer was **{correctAnswerIndex + 1}. {q.CorrectAnswer}**")
                                .WithColor(DiscordColor.Red)));
                    }
                }
            }

        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
            .WithTitle("Game over!")
            .WithDescription($"You got **{score}** out of **{numRounds}** questions correct!")
            .WithColor(DiscordColor.Blurple)));
    }
}