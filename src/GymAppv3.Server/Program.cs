using GymAppv3.Server.Configuration;
using GymAppv3.Server.Endpoints.Auth;
using GymAppv3.Server.Endpoints.Booking;
using GymAppv3.Server.Endpoints.ClassCategory;
using GymAppv3.Server.Endpoints.ClassRoom;
using GymAppv3.Server.Endpoints.ClassSession;
using GymAppv3.Server.Endpoints.GymBuilding;
using GymAppv3.Server.Endpoints.Member;
using GymAppv3.Server.Endpoints.Membership;
using GymAppv3.Server.Endpoints.MembershipPackage;
using GymAppv3.Server.Endpoints.Payment;
using GymAppv3.Server.Endpoints.Trainer;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure application services
builder.ConfigureApplication();
builder.ConfigureRateLimiting();

// Add endpoints API explorer for OpenAPI
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Seed roles on application startup

// Initialize database in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            // Apply any pending migrations and create database if it doesn't exist
            await context.Database.MigrateAsync();

            Console.WriteLine("Database created and migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred creating the database: {ex.Message}");
        }
    }

    // Seed roles after database is ready
    await SeedData.InitializeRolesAsync(app.Services);

    // Enable Scalar documentation in development
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("GymAppv3 API");
    });
}


app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapAuthEndpoints();
app.MapGymBuildingEndpoints();
app.MapClassCategoryEndpoints();
app.MapClassRoomEndpoints();
app.MapClassSessionEndpoints();
app.MapMembershipPackageEndpoints();
app.MapMemberEndpoints();
app.MapBookingEndpoints();
app.MapMembershipEndpoints();
app.MapPaymentEndpoints();
app.MapTrainerEndpoints();

app.Run();
