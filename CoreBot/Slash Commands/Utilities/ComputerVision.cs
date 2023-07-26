using DSharpPlus.SlashCommands;
using Azure;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;
using DSharpPlus.Entities;

namespace CoreBot.Slash_Commands.Utilities;

public class ComputerVision : ApplicationCommandModule
{
    [SlashCommand("DescribeImage", "Uses computer vision to describe the contents of an image")]
    public async Task AnalyzeImage(InteractionContext ctx,
        [Option("Image", "The Image you want described")] DiscordAttachment image)
    {
        var serviceOptions = new VisionServiceOptions(
            Configuration.VisionEndpoint,
            new AzureKeyCredential(Configuration.VisionKey));

        using var imageSource = VisionSource.FromUrl(
            new Uri(image.ProxyUrl));

        var analysisOptions = new ImageAnalysisOptions
        {
            Features = ImageAnalysisFeature.Caption | ImageAnalysisFeature.Text,
            Language = "en",
            GenderNeutralCaption = true
        };

        using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

        var result =  analyzer.Analyze();
        if (result.Reason == ImageAnalysisResultReason.Analyzed)
        {
            if (result.Caption != null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Caption:")
                    .WithColor(DiscordColor.Azure)
                    .WithDescription($"## {result.Caption.Content}\nConfidence: {result.Caption.Confidence:0.00}")
                    .WithImageUrl(image.Url);
                await ctx.CreateResponseAsync(embed:embed);

            }

            if (result.Text != null)
            {
                foreach (var line in result.Text.Lines)
                {
                    string pointsToString = "{" + String.Join(',',
                        line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
                    await ctx.CreateResponseAsync($"Line: '{line.Content}', Bounding polygon {pointsToString}");

                    foreach (var word in line.Words)
                    {
                        pointsToString = "{" + string.Join(',', word.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
                        await ctx.CreateResponseAsync(
                            $"Word: '{word.Content}', Bounding polygon {pointsToString}, Confidence {word.Confidence:0.00}");
                    }
                }
            }
        }
        else
        {
            var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
            var errorEmbed = new DiscordEmbedBuilder()
                .WithTitle("Analysis failed")
                .WithColor(DiscordColor.Red)
                .WithDescription(
                    $"Error reason: {errorDetails.Reason}\nError code: {errorDetails.ErrorCode}\nError message: {errorDetails.Message}");

            await ctx.CreateResponseAsync(embed: errorEmbed);
        }
    }
}