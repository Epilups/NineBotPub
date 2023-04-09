namespace CoreBot;

internal class Program
{
    static void Main(string[] args)
    {
        string port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"https://*:{port}");
        var bot = new Bot();
        bot.RunAsync().GetAwaiter().GetResult();
    }
}