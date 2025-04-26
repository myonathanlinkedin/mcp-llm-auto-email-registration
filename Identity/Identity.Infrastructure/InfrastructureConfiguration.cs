using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddIdentity()
            .AddDBStorage<IdentityDbContext>(configuration, Assembly.GetExecutingAssembly())
            .AddScoped<IDbInitializer, IdentityDbInitializer>(); // Scoped because it's a request-based service

    private static IServiceCollection AddIdentity(
        this IServiceCollection services)
    {
        services
            .AddScoped<IIdentity, IdentityService>() // Scoped for login/register/reset operations
            .AddScoped<IJwtGenerator, JwtGeneratorService>() // Scoped for token generation
            .AddSingleton<IRsaKeyProvider, RsaKeyProviderService>() // Singleton as it's stateless
            .AddScoped<IEmailSender, EmailSenderService>() // Scoped for sending emails during registration/reset
            .AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = CommonModelConstants.Identity.MinPasswordLength;
            })
            .AddEntityFrameworkStores<IdentityDbContext>();

        return services;
    }
}
