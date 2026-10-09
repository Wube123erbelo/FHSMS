syntax=docker/dockerfile:1
============================================================
BUILD STAGE
============================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

Copy project files first for Docker layer caching

COPY src/FHSMS.Domain/FHSMS.Domain.csproj src/FHSMS.Domain/ COPY src/FHSMS.Application/FHSMS.Application.csproj src/FHSMS.Application/ COPY src/FHSMS.Infrastructure/FHSMS.Infrastructure.csproj src/FHSMS.Infrastructure/ COPY src/FHSMS.API/FHSMS.API.csproj src/FHSMS.API/

Restore NuGet dependencies

RUN dotnet restore src/FHSMS.API/FHSMS.API.csproj

Copy application source code

COPY src/ src/

Publish the API

RUN dotnet publish src/FHSMS.API/FHSMS.API.csproj
--configuration Release
--output /app/publish
--no-restore

============================================================
RUNTIME STAGE
============================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

Listen on the assigned container port

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

Copy published application

COPY --from=build /app/publish .

Start the API

ENTRYPOINT ["dotnet", "FHSMS.API.dll"]