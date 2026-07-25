using FluentValidation;
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Data.Interceptors;
using GymAppV3.Infrastructure.Handlers;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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

        // Infrastructure services
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddScoped<IVatRateProvider, VatRateProvider>();

        // Business services — register each concrete once, share the instance across its interfaces.
        // Without this, each interface registration creates its own instance per request.

        // Business services - Gym Building
        builder.Services.AddScopedShared<GymBuildingService, IGymBuildingCommandService, IGymBuildingQueryService>();

        // Business services - Class Category
        builder.Services.AddScopedShared<ClassCategoryService, IClassCategoryCommandService, IClassCategoryQueryService>();

        // Business services - Class Room
        builder.Services.AddScopedShared<ClassRoomService, IClassRoomCommandService, IClassRoomQueryService>();

        // Business services - Class Session
        builder.Services.AddScopedShared<ClassSessionService, IClassSessionCommandService, IClassSessionQueryService>();

        // Business services - Membership Package
        builder.Services.AddScopedShared<MembershipPackageService, IMembershipPackageCommandService, IMembershipPackageQueryService>();

        // Business services - Member
        builder.Services.AddScopedShared<MemberService, IMemberCommandService, IMemberQueryService>();

        // Business services - Membership
        builder.Services.AddScopedShared<MembershipService, IMembershipCommandService, IMembershipQueryService>();

        // Business services - Booking
        builder.Services.AddScopedShared<BookingService, IBookingCommandService, IBookingQueryService>();

        // Business services - Payment
        builder.Services.AddScopedShared<PaymentService, IPaymentCommandService, IPaymentQueryService>();

        // Business services - Trainer
        builder.Services.AddScopedShared<TrainerService, ITrainerCommandService, ITrainerQueryService>();

        // Auto-discover all AbstractValidator<T> in the Core assembly.
        // New validators added under Core/Validators/ are picked up without touching this line.
        builder.Services.AddValidatorsFromAssemblyContaining<GymAppV3.Core.Commands.ScheduleClassSessionCommand>();
    }
}
