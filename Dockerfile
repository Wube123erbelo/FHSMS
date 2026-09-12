# syntax=docker/dockerfile:1

# --- Build stage -------------------------------------------------------
# Builds and publishes FHSMS.API and everything it depends on (Domain,
# Application, Infrastructure). Only the .csproj files are copied before
# `dotnet restore` so Docker's layer cache can skip the restore step
# entirely on rebuilds where only .cs files changed - the biggest lever for
# fast iterative builds here.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FHSMS.Domain/FHSMS.Domain.csproj src/FHSMS.Domain/
COPY src/FHSMS.Application/FHSMS.Application.csproj src/FHSMS.Application/
COPY src/FHSMS.Infrastructure/FHSMS.Infrastructure.csproj src/FHSMS.Infrastructure/
COPY src/FHSMS.API/FHSMS.API.csproj src/FHSMS.API/

RUN dotnet restore src/FHSMS.API/FHSMS.API.csproj

COPY src/ src/
RUN dotnet publish src/FHSMS.API/FHSMS.API.csproj -c Release -o /app/publish --no-restore

# --- Runtime stage -------------------------------------------------------
# The much smaller ASP.NET runtime image (no SDK/compiler) - this is the
# image Render actually deploys and runs.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Runs as the image's built-in non-root user rather than root.
USER app

COPY --from=build /app/publish .

# Informational only - Render assigns the real port via the PORT env var,
# which Program.cs reads and binds Kestrel to at startup.
EXPOSE 8080

ENTRYPOINT ["dotnet", "FHSMS.API.dll"]
