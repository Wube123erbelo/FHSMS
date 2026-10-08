# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/FHSMS.Domain/FHSMS.Domain.csproj src/FHSMS.Domain/
COPY src/FHSMS.Application/FHSMS.Application.csproj src/FHSMS.Application/
COPY src/FHSMS.Infrastructure/FHSMS.Infrastructure.csproj src/FHSMS.Infrastructure/
COPY src/FHSMS.API/FHSMS.API.csproj src/FHSMS.API/

RUN dotnet restore src/FHSMS.API/FHSMS.API.csproj

COPY src/ src/

RUN dotnet publish src/FHSMS.API/FHSMS.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "FHSMS.API.dll"]