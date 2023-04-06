using Newtonsoft.Json;

namespace CoreBot.External_Classes;

internal struct UrbanDictionaryResult
{
    [JsonProperty("list")]
    public List<UrbanDictionaryEntry> List { get; set; }
}

public class UrbanDictionaryEntry
{
    [JsonProperty("definition")]
    public string? Definition { get; set; }

    [JsonProperty("example")]
    public string? Example { get; set; }

    [JsonProperty("thumbs_up")]
    public int ThumbsUp { get; set; }

    [JsonProperty("thumbs_down")]
    public int ThumbsDown { get; set; }

    [JsonProperty("author")]
    public string? Author { get; set; }

    [JsonProperty("word")]
    public string? Word { get; set; }

    [JsonProperty("permalink")]
    public string? Permalink { get; set; }

    [JsonProperty("sound_urls")]
    public List<string>? SoundUrls { get; set; }

    [JsonProperty("current_vote")]
    public string? CurrentVote { get; set; }
}