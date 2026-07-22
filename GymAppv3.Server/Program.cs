using GymAppv3.Server.Configuration;
using GymAppv3.Server.Endpoints.Auth;
using GymAppv3.Server.Endpoints.Booking;
using GymAppv3.Server.Endpoints.ClassCategory;
using GymAppv3.Server.Endpoints.ClassRoom;
using GymAppv3.Server.Endpoints.ClassSession;
using GymAppv3.Server.Endpoints.GymBuilding;
using GymAppv3.Server.Endpoints.Membership;
using GymAppv3.Server.Endpoints.MembershipPackage;
using GymAppv3.Server.Endpoints.Payment;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure application services
builder.ConfigureApplication();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymAppv3 API",
        Version = "v1",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GymAppv3 API v1");
        options.RoutePrefix = "swagger"; // Set Swagger UI at the app's root (http://localhost:<port>/swagger)
    });
}


app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGymBuildingEndpoints();
app.MapClassCategoryEndpoints();
app.MapClassRoomEndpoints();
app.MapClassSessionEndpoints();
app.MapMembershipPackageEndpoints();
app.MapMembershipEndpoints();
app.MapBookingEndpoints();
app.MapPaymentEndpoints();

app.Run();
