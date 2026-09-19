using System.Net.Http.Json;
using FHSMS.TelegramBot.Clients.Models;
using FHSMS.TelegramBot.Configuration;
using Microsoft.Extensions.Options;

namespace FHSMS.TelegramBot.Clients;

/// <summary>
/// Thin wrapper around the Telegram Bot API. Uses long polling (getUpdates) so
/// the bot needs no public HTTPS endpoint or webhook registration - just a bot
/// token from @BotFather and outbound internet access.
/// </summary>
public class TelegramApiClient
{
    private readonly HttpClient _http;
    private readonly TelegramSettings _settings;

    public TelegramApiClient(HttpClient http, IOptions<TelegramSettings> settings)
    {
        _settings = settings.Value;
        if (string.IsNullOrWhiteSpace(_settings.BotToken))
            throw new InvalidOperationException("Telegram:BotToken is not configured. Set it via user-secrets or environment variables.");

        http.BaseAddress = new Uri($"https://api.telegram.org/bot{_settings.BotToken}/");
        http.Timeout = TimeSpan.FromSeconds(_settings.PollTimeoutSeconds + 15);
        _http = http;
    }

    public async Task<List<TelegramUpdate>> GetUpdatesAsync(long offset, CancellationToken cancellationToken)
    {
        var url = $"getUpdates?offset={offset}&timeout={_settings.PollTimeoutSeconds}";
        var response = await _http.GetFromJsonAsync<TelegramGetUpdatesResponse>(url, cancellationToken);
        return response?.Result ?? new List<TelegramUpdate>();
    }

    public async Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
        await _http.PostAsJsonAsync("sendMessage", new { chat_id = chatId, text }, cancellationToken);
    }
}
