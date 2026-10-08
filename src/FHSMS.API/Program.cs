using FHSMS.Application;
using FHSMS.Application.Common.Exceptions;
using FHSMS.Infrastructure;
using FHSMS.Infrastructure.Persistence;
using FHSMS.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVER / PORT CONFIGURATION
// ============================================================
// AletCloud provides PORT at runtime.
// Default to 8080 if PORT is not provided.
//
// IMPORTANT:
// 0.0.0.0 makes the API reachable from outside the container.
// ============================================================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// ============================================================
// DATABASE CONFIGURATION
// ============================================================
// Supports DATABASE_URL such as:
//
// postgres://user:password@host:5432/database
//
// and converts it to an Npgsql connection string.
// ============================================================

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    try
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            ConvertPostgresUrlToNpgsqlConnectionString(databaseUrl);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DATABASE_URL could not be parsed: {ex.Message}");
        throw;
    }
}


// ============================================================
// CONTROLLERS / JSON
// ============================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FHSMS API",
        Version = "v1",
        Description = "FHSMS Backend API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    var allowedOrigins =
        Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? builder.Configuration["Cors:AllowedOrigins"];

    var origins = allowedOrigins?
        .Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries
        );

    options.AddPolicy("Default", policy =>
    {
        if (origins is { Length: > 0 })
        {
            policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            // Temporary fallback while the deployment is being configured.
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});


// ============================================================
// APPLICATION / INFRASTRUCTURE
// ============================================================

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// GLOBAL EXCEPTION HANDLER
// ============================================================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature =
            context.Features.Get<IExceptionHandlerFeature>();

        var exception = feature?.Error;

        var (statusCode, payload) = exception switch
        {
            ValidationException validationEx =>
                (
                    StatusCodes.Status400BadRequest,
                    (object)validationEx.Errors
                ),

            NotFoundException notFoundEx =>
                (
                    StatusCodes.Status404NotFound,
                    (object)new
                    {
                        message = notFoundEx.Message
                    }
                ),

            UnauthorizedAccessException authEx =>
                (
                    StatusCodes.Status401Unauthorized,
                    (object)new
                    {
                        message = authEx.Message
                    }
                ),

            FHSMS.Domain.Exceptions.DomainException domainEx =>
                (
                    StatusCodes.Status400BadRequest,
                    (object)new
                    {
                        message = domainEx.Message
                    }
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    (object)new
                    {
                        message = "An unexpected error occurred."
                    }
                )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(payload);
    });
});


// ============================================================
// SWAGGER
// ============================================================
// Enabled in production so we can test the deployed API.
// ============================================================

app.UseSwagger();

app.UseSwaggerUI();


// ============================================================
// CORS
// ============================================================

app.UseCors("Default");


// ============================================================
// AUTHENTICATION / AUTHORIZATION
// ============================================================

app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// USER PRESENCE TRACKING
// ============================================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userIdClaim =
            context.User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            try
            {
                using var scope =
                    context.RequestServices.CreateScope();

                var db =
                    scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                await db.Users
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            u => u.LastSeenAt,
                            DateTime.UtcNow
                        )
                    );
            }
            catch (Exception ex)
            {
                // Do not break an otherwise valid API request
                // just because presence tracking failed.
                Console.WriteLine(
                    $"Presence tracking failed: {ex.Message}"
                );
            }
        }
    }

    await next();
});


// ============================================================
// API CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// HEALTH CHECK
// ============================================================
// This endpoint does NOT require the database.
// It lets us confirm that the container itself is alive.
// ============================================================

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        service = "FHSMS API",
        environment =
            Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT"
            ) ?? "Production",
        port = port
    });
});


// ============================================================
// ROOT ENDPOINT
// ============================================================

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "FHSMS API",
        status = "running",
        health = "/health",
        swagger = "/swagger"
    });
});


// ============================================================
// DATABASE SEEDING
// ============================================================
//
// IMPORTANT:
// We are intentionally NOT running the database seeder during
// startup right now.
//
// First we need to prove that AletCloud can start the API.
// Once /health works, we can safely configure migrations and
// database seeding.
//
// ============================================================


// ============================================================
// START APPLICATION
// ============================================================

Console.WriteLine("==========================================");
Console.WriteLine("FHSMS API starting...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Port: {port}");
Console.WriteLine($"Listening on: http://0.0.0.0:{port}");
Console.WriteLine("==========================================");

app.Run();


// ============================================================
// POSTGRES URL CONVERTER
// ============================================================

static string ConvertPostgresUrlToNpgsqlConnectionString(string url)
{
    var uri = new Uri(url);

    var userInfo = uri.UserInfo.Split(':', 2);

    if (userInfo.Length == 0 || string.IsNullOrWhiteSpace(userInfo[0]))
    {
        throw new InvalidOperationException(
            "DATABASE_URL does not contain a PostgreSQL username."
        );
    }

    var database = uri.AbsolutePath.TrimStart('/');

    if (string.IsNullOrWhiteSpace(database))
    {
        throw new InvalidOperationException(
            "DATABASE_URL does not contain a database name."
        );
    }

    var query =
        Microsoft.AspNetCore.WebUtilities.QueryHelpers
            .ParseQuery(uri.Query);

    var sslMode =
        query.TryGetValue("sslmode", out var sslModeValues)
            ? sslModeValues.ToString()
            : null;

    var npgsqlBuilder =
        new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,

            Port = uri.Port > 0
                ? uri.Port
                : 5432,

            Username =
                Uri.UnescapeDataString(userInfo[0]),

            Password =
                userInfo.Length > 1
                    ? Uri.UnescapeDataString(userInfo[1])
                    : "",

            Database =
                Uri.UnescapeDataString(database),

            Pooling = true
        };

    if (
        string.IsNullOrEmpty(sslMode) ||
        sslMode.Equals(
            "require",
            StringComparison.OrdinalIgnoreCase
        )
    )
    {
        npgsqlBuilder.SslMode = SslMode.Require;
        npgsqlBuilder.TrustServerCertificate = true;
    }

    return npgsqlBuilder.ConnectionString;
}