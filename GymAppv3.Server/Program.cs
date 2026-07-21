using GymAppv3.Server.Configuration;
using GymAppv3.Server.Endpoints.Auth;
using GymAppv3.Server.Endpoints.ClassCategory;
using GymAppv3.Server.Endpoints.ClassRoom;
using GymAppv3.Server.Endpoints.GymBuilding;
using GymAppv3.Server.Endpoints.MembershipPackage;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.ConfigureApplication();

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
            await context.Database.EnsureCreatedAsync();

            Console.WriteLine("Database created and migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred creating the database: {ex.Message}");
        }
    }

    // Seed roles after database is ready
    await SeedData.InitializeRolesAsync(app.Services);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGymBuildingEndpoints();
app.MapClassCategoryEndpoints();
app.MapClassRoomEndpoints();
app.MapMembershipPackageEndpoints();

app.Run();
