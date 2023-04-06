using Newtonsoft.Json;

namespace CoreBot.External_Classes;

public class TriviaResults
{
    [JsonProperty("category")]
    public string? Category { get; set; }
        
    [JsonProperty("difficulty")]
    public string? Difficulty { get; set; }
        
    [JsonProperty("question")]
    public string? Question { get; set; }
        
    [JsonProperty("correctAnswer")]
    public string? CorrectAnswer { get; set; }
        
    [JsonProperty("incorrectAnswers")]
    public string[]? IncorrectAnswers { get; set; }
        
}