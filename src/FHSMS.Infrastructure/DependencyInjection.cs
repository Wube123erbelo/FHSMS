using System.Text;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Infrastructure.Identity;
using FHSMS.Infrastructure.PaymentProviders;
using FHSMS.Infrastructure.Persistence;
using FHSMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FHSMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<IDocumentNumberGenerator, DocumentNumberGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.Configure<PaymentProviderSettings>(configuration.GetSection(PaymentProviderSettings.SectionName));
        services.AddScoped<IPaymentProvider, BankTransferPaymentProvider>();
        services.AddScoped<IPaymentProvider, TelebirrPaymentProvider>();
        // Chapa needs an HttpClient (initiate + verify calls to api.chapa.co)
        // - registered as a typed client on the concrete class, then exposed
        // through IPaymentProvider by resolving that same typed-client
        // instance, so both resolution paths return one consistent object.
        services.AddHttpClient<ChapaPaymentProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.chapa.co/v1/");
        });
        services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<ChapaPaymentProvider>());
        services.AddScoped<IPaymentProvider, CbeBirrPaymentProvider>();
        services.AddScoped<IPaymentProviderRegistry, PaymentProviderRegistry>();

        // Receipt verifiers - the "I already paid, here's my reference"
        // self-service path (distinct from the hosted-checkout-redirect
        // providers above). See IReceiptVerifier's remarks.
        services.AddHttpClient<PaymentProviders.ReceiptVerifiers.CbeBirrReceiptVerifier>();
        services.AddScoped<IReceiptVerifier>(sp => sp.GetRequiredService<PaymentProviders.ReceiptVerifiers.CbeBirrReceiptVerifier>());
        services.AddScoped<IReceiptVerifierRegistry, PaymentProviders.ReceiptVerifiers.ReceiptVerifierRegistry>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        services.AddAuthorization();

        return services;
    }
}
