# syntax=docker/dockerfile:1

# Deployed on Render as a Background Worker (no HTTP port, no health check
# path needed - it's a long-running console host that polls Telegram).

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY telegram-bot/src/FHSMS.TelegramBot.csproj telegram-bot/src/
RUN dotnet restore telegram-bot/src/FHSMS.TelegramBot.csproj

COPY telegram-bot/src/ telegram-bot/src/
RUN dotnet publish telegram-bot/src/FHSMS.TelegramBot.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FHSMS.TelegramBot.dll"]
