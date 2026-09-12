namespace FHSMS.TelegramBot.Configuration;

public class TelegramSettings
{
    public const string SectionName = "Telegram";

    public string BotToken { get; set; } = default!;
    public int PollTimeoutSeconds { get; set; } = 30;
}
