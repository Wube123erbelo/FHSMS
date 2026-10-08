namespace FHSMS.TelegramBot.Conversation;

public interface IBotStateStore
{
    BotSession GetOrCreate(long chatId);
}
