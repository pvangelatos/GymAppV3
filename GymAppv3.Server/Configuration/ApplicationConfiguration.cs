using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Infrastructure.Data;
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

        // Database
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure Identity and Authentication
        builder.ConfigureIdentity();

        // HTTP Context Accessor (required for UserContext)
        builder.Services.AddHttpContextAccessor();

        // Infrastructure services
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddScoped<IVatRateProvider, VatRateProvider>();

        // Business services - Gym Building
        builder.Services.AddScoped<IGymBuildingCommandService, GymBuildingService>();
        builder.Services.AddScoped<IGymBuildingQueryService, GymBuildingService>();

        // Business services - Class Category
        builder.Services.AddScoped<IClassCategoryCommandService, ClassCategoryService>();
        builder.Services.AddScoped<IClassCategoryQueryService, ClassCategoryService>();

        // Business services - Class Room
        builder.Services.AddScoped<IClassRoomCommandService, ClassRoomService>();
        builder.Services.AddScoped<IClassRoomQueryService, ClassRoomService>();

        // Business services - Class Session
        builder.Services.AddScoped<IClassSessionCommandService, ClassSessionService>();
        builder.Services.AddScoped<IClassSessionQueryService, ClassSessionService>();

        // Business services - Membership Package
        builder.Services.AddScoped<IMembershipPackageCommandService, MembershipPackageService>();
        builder.Services.AddScoped<IMembershipPackageQueryService, MembershipPackageService>();

        // Business services - Member
        builder.Services.AddScoped<IMemberCommandService, MemberService>();
        builder.Services.AddScoped<IMemberQueryService, MemberService>();

        // Business services - Membership
        builder.Services.AddScoped<IMembershipCommandService, MembershipService>();
        builder.Services.AddScoped<IMembershipQueryService, MembershipService>();

        // Business services - Booking
        builder.Services.AddScoped<IBookingCommandService, BookingService>();
        builder.Services.AddScoped<IBookingQueryService, BookingService>();

        // Business services - Payment
        builder.Services.AddScoped<IPaymentCommandService, PaymentService>();
        builder.Services.AddScoped<IPaymentQueryService, PaymentService>();

        // Business services - Trainer
        builder.Services.AddScoped<ITrainerCommandService, TrainerService>();
        builder.Services.AddScoped<ITrainerQueryService, TrainerService>();
    }
}
