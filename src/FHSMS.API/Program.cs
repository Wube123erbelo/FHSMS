using FHSMS.Application;
using FHSMS.Application.Common.Exceptions;
using FHSMS.Infrastructure;
using FHSMS.Infrastructure.Persistence;
using FHSMS.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;

// ============================================================
// SERVER / PORT CONFIGURATION
// ============================================================

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ============================================================
// DATABASE CONFIGURATION
// ============================================================

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        ConvertPostgresUrlToNpgsqlConnectionString(databaseUrl);
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

builder.Services.AddEndpointsApiExplorer();

// ============================================================
// SWAGGER / JWT
// ============================================================

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

    var origins = allowedOrigins?.Split(
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
            // Convenient for initial deployment.
            // Configure ALLOWED_ORIGINS for production.
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
                    (object)new { message = notFoundEx.Message }
                ),

            UnauthorizedAccessException authEx =>
                (
                    StatusCodes.Status401Unauthorized,
                    (object)new { message = authEx.Message }
                ),

            FHSMS.Domain.Exceptions.DomainException domainEx =>
                (
                    StatusCodes.Status400BadRequest,
                    (object)new { message = domainEx.Message }
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

// Enabled in production to allow deployment testing.
// Restrict access if your API documentation should be private.

app.UseSwagger();
app.UseSwaggerUI();

// ============================================================
// CORS / AUTHENTICATION
// ============================================================

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// USER PRESENCE TRACKING
// ============================================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userIdClaim = context.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        )?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            try
            {
                using var scope =
                    context.RequestServices.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                await db.Users
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(
                            u => u.LastSeenAt,
                            DateTime.UtcNow
                        )
                    );
            }
            catch (Exception ex)
            {
                // Presence tracking should not break an API request.
                app.Logger.LogError(
                    ex,
                    "Failed to update user presence."
                );
            }
        }
    }

    await next();
});

// ============================================================
// ENDPOINTS
// ============================================================

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "FHSMS API",
    status = "running",
    health = "/health",
    swagger = "/swagger"
}));

// Liveness check: does not require a database connection.
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "FHSMS API",
    environment = app.Environment.EnvironmentName,
    port
}));

// ============================================================
// DATABASE SEEDING
// ============================================================

// Preserve the existing seeding behavior.
// A seeding failure is logged so it does not automatically prevent
// the API from starting. Database-dependent endpoints may still fail.

try
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await ApplicationDbContextSeeder.SeedAsync(db);

    app.Logger.LogInformation("Database seeding completed.");
}
catch (Exception ex)
{
    app.Logger.LogError(
        ex,
        "Database seeding failed. Check the database configuration."
    );
}

// ============================================================
// START APPLICATION
// ============================================================

app.Logger.LogInformation(
    "Starting FHSMS API on port {Port}. Environment: {Environment}",
    port,
    app.Environment.EnvironmentName
);

app.Run();

// ============================================================
// POSTGRESQL URL CONVERTER
// ============================================================

static string ConvertPostgresUrlToNpgsqlConnectionString(string url)
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" &&
         uri.Scheme != "postgresql"))
    {
        throw new InvalidOperationException(
            "DATABASE_URL must be a valid postgres:// or postgresql:// URL."
        );
    }

    var userInfo = uri.UserInfo.Split(':', 2);

    if (userInfo.Length == 0 ||
        string.IsNullOrWhiteSpace(userInfo[0]))
    {
        throw new InvalidOperationException(
            "DATABASE_URL does not contain a PostgreSQL username."
        );
    }

    var database = Uri.UnescapeDataString(
        uri.AbsolutePath.TrimStart('/')
    );

    if (string.IsNullOrWhiteSpace(database))
    {
        throw new InvalidOperationException(
            "DATABASE_URL does not contain a database name."
        );
    }

    var query =
        Microsoft.AspNetCore.WebUtilities.QueryHelpers
            .ParseQuery(uri.Query);

    var sslModeValue =
        query.TryGetValue("sslmode", out var values)
            ? values.ToString()
            : null;

    var connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : "",
        Database = database,
        Pooling = true
    };

    if (string.IsNullOrWhiteSpace(sslModeValue) ||
        sslModeValue.Equals(
            "require",
            StringComparison.OrdinalIgnoreCase))
    {
        connectionString.SslMode = SslMode.Require;
        connectionString.TrustServerCertificate = true;
    }
    else if (Enum.TryParse<SslMode>(
        sslModeValue,
        true,
        out var sslMode))
    {
        connectionString.SslMode = sslMode;
    }
    else
    {
        throw new InvalidOperationException(
            $"Unsupported PostgreSQL sslmode: {sslModeValue}"
        );
    }

    return connectionString.ConnectionString;
}