using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GymAppV3.Infrastructure.DependencyInjection;

public static class GymAppServiceCollectionExtensions
{
    public static IServiceCollection AddGymAppDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IVatRateProvider, VatRateProvider>();

        services.AddScopedShared<GymBuildingService, IGymBuildingCommandService, IGymBuildingQueryService>();
        services.AddScopedShared<ClassCategoryService, IClassCategoryCommandService, IClassCategoryQueryService>();
        services.AddScopedShared<ClassRoomService, IClassRoomCommandService, IClassRoomQueryService>();
        services.AddScopedShared<ClassSessionService, IClassSessionCommandService, IClassSessionQueryService>();
        services.AddScopedShared<MembershipPackageService, IMembershipPackageCommandService, IMembershipPackageQueryService>();
        services.AddScopedShared<MemberService, IMemberCommandService, IMemberQueryService>();
        services.AddScopedShared<MembershipService, IMembershipCommandService, IMembershipQueryService>();
        services.AddScopedShared<BookingService, IBookingCommandService, IBookingQueryService>();
        services.AddScopedShared<PaymentService, IPaymentCommandService, IPaymentQueryService>();
        services.AddScopedShared<TrainerService, ITrainerCommandService, ITrainerQueryService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}