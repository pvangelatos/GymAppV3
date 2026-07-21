using GymAppV3.Core.Interfaces;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Handlers;
using GymAppV3.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GymAppv3.Server.Configuration;

public static class ApplicationConfiguration
{
    public static void ConfigureApplication(this WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddOpenApi();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IGymBuildingCommandService, GymBuildingService>();
        builder.Services.AddScoped<IGymBuildingQueryService, GymBuildingService>();
    }
}
