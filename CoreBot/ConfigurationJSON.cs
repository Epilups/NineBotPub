using Newtonsoft.Json;

namespace CoreBot;

internal struct ConfigurationJSON
{
    [JsonProperty("token")]
    public string Token { get; private set; }

    [JsonProperty("prefix")]
    public string Prefix { get; private set; }
        
    [JsonProperty("api_key")]
    public string ApiKey { get; private set; }
        
    [JsonProperty("paula_key")]
    public string PaulaKey { get; private set; }
        
    [JsonProperty("username")]
    public string Username { get; private set; }
        
    [JsonProperty("password")]
    public string Password { get; private set; }
        
    [JsonProperty("dan")]
    public string Dan { get; private set; }
        
    [JsonProperty("devmode")]
    public string Devmode { get; private set; }
        
    [JsonProperty("mongouser")]
    public string MongoUser { get; private set; }
        
    [JsonProperty("mongopassword")]
    public string MongoPassword { get; private set; }
}