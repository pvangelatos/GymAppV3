using FluentValidation;
using GymAppV3.Core.Abstractions;
using GymAppV3.Infrastructure.Data.Interceptors;
using GymAppV3.Infrastructure.DependencyInjection;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = GymAppV3.Infrastructure.Data.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register the interceptor as scoped
builder.Services.AddScoped<AuditableEntityInterceptor>();

// Use the shared ApplicationDbContext from Infrastructure
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// HTTP Context Accessor (required for UserContext)
builder.Services.AddHttpContextAccessor();

// Infrastructure services
builder.Services.AddGymAppDomainServices();

// Configure authorization policies using the extension method
builder.Services.AddAuthorization(options =>
{
    options.AddGymAppPolicies();
});

// Reuse the same Core command validators already used by the Server API.
// Deliberately NOT setting Greek culture here (unlike Server) — keeps English
// messages, consistent with this session's English-only UI decision.
builder.Services.AddValidatorsFromAssembly(typeof(GymAppV3.Core.Commands.ScheduleClassSessionCommand).Assembly);

builder.Services.AddRazorPages();

var app = builder.Build();

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync(); // Ensure the database is created and migrations are applied

    // Create roles if they don't exist
    string[] roleNames = 
    { 
        RoleConstants.Member, 
        RoleConstants.Trainer, 
        RoleConstants.Admin, 
        RoleConstants.TrainerAdmin 
    };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default admin user if configured
    var adminEmail = builder.Configuration["DefaultAdmin:Email"]
        ?? throw new InvalidOperationException("Default admin email is not configured.");
    var adminPassword = builder.Configuration["DefaultAdmin:Password"]
        ?? throw new InvalidOperationException("Default admin password is not configured.");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if(adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newAdmin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, RoleConstants.Admin);
            }
        
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
