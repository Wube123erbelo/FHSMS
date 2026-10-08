using System.Collections.Concurrent;

namespace FHSMS.TelegramBot.Conversation;

public class InMemoryBotStateStore : IBotStateStore
{
    private readonly ConcurrentDictionary<long, BotSession> _sessions = new();

    public BotSession GetOrCreate(long chatId) =>
        _sessions.GetOrAdd(chatId, id => new BotSession { ChatId = id });
}
