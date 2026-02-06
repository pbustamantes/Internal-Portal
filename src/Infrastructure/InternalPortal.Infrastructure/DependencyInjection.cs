using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InternalPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddSignalR();

        return services;
    }
}
