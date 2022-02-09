namespace AlertBot.WebApi.Models;

public class UserConfiguration
{
    public long Id { get; set; }
    public long? ChatId { get; set; }
    public string? BinanceApiKey { get; set; }
    public string? BinanceApiSecret { get; set; }
}