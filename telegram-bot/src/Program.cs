using FHSMS.TelegramBot.Clients;
using FHSMS.TelegramBot.Configuration;
using FHSMS.TelegramBot.Conversation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection(TelegramSettings.SectionName));
builder.Services.Configure<FhsmsSettings>(builder.Configuration.GetSection(FhsmsSettings.SectionName));

builder.Services.AddHttpClient<TelegramApiClient>();
builder.Services.AddHttpClient<FhsmsApiClient>();

builder.Services.AddSingleton<IBotStateStore, InMemoryBotStateStore>();
builder.Services.AddSingleton<ConversationHandler>();

var host = builder.Build();

var telegram = host.Services.GetRequiredService<TelegramApiClient>();
var fhsms = host.Services.GetRequiredService<FhsmsApiClient>();
var handler = host.Services.GetRequiredService<ConversationHandler>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

Console.WriteLine("FHSMS Telegram bot starting...");
Console.WriteLine("Logging in to FHSMS API as the configured service account...");
await fhsms.LoginServiceAccountAsync(cts.Token);
Console.WriteLine("Logged in. Polling Telegram for updates (Ctrl+C to stop)...");

long offset = 0;

while (!cts.Token.IsCancellationRequested)
{
    try
    {
        var updates = await telegram.GetUpdatesAsync(offset, cts.Token);

        foreach (var update in updates)
        {
            offset = update.UpdateId + 1;

            if (update.Message?.Text is not null)
            {
                Console.WriteLine($"[chat {update.Message.Chat.Id}] {update.Message.Text}");
                await handler.HandleAsync(update.Message, cts.Token);
            }
        }
    }
    catch (OperationCanceledException)
    {
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error while polling Telegram: {ex.Message}");
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
}

Console.WriteLine("FHSMS Telegram bot stopped.");
