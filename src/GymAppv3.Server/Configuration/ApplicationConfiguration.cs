using FluentValidation;
using GymAppV3.Core.Abstractions;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Data.Interceptors;
using GymAppV3.Infrastructure.DependencyInjection;
using GymAppV3.Infrastructure.Handlers;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GymAppv3.Server.Configuration;

public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Register the interceptor as scoped - it depends on IUserContext which is scoped
        builder.Services.AddScoped<AuditableEntityInterceptor>();

        // Database - resolve interceptor from DI so IDateTimeProvider / IUserContext get wired
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        // Configure Identity and Authentication
        builder.ConfigureIdentity();

        // HTTP Context Accessor (required for UserContext)
        builder.Services.AddHttpContextAccessor();

        // Register the UserContext service
        builder.Services.AddGymAppDomainServices();

        // All FluentValidation default messages switches to Greek.
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("el-GR");

        // Scan both Core (for command validators) and Server (for wire-request validators).
        builder.Services.AddValidatorsFromAssemblies(new[]
        {
            typeof(GymAppV3.Core.Commands.ScheduleClassSessionCommand).Assembly,
            typeof(Program).Assembly
        });
    }
}
