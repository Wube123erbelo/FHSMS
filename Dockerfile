# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY src/FHSMS.Domain/FHSMS.Domain.csproj src/FHSMS.Domain/
COPY src/FHSMS.Application/FHSMS.Application.csproj src/FHSMS.Application/
COPY src/FHSMS.Infrastructure/FHSMS.Infrastructure.csproj src/FHSMS.Infrastructure/
COPY src/FHSMS.API/FHSMS.API.csproj src/FHSMS.API/

# Restore dependencies
RUN dotnet restore src/FHSMS.API/FHSMS.API.csproj

# Copy source code
COPY src/ src/

# Publish API
RUN dotnet publish src/FHSMS.API/FHSMS.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FHSMS.API.dll"]