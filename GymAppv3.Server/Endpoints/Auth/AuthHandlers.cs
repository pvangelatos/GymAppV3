using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GymAppv3.Server.Endpoints.Auth;

public static class AuthHandlers
{
    /// <summary>
    /// Registers a new user with Member role and creates a Member profile
    /// </summary>
    public static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Password and confirmation password do not match."
            });
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Results.BadRequest(new AuthResponse
            {
                Success = false,
                Message = "User with this email already exists."
            });
        }

        // Create Identity user
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true // Set to false if you want email verification
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(new AuthResponse
            {
                Success = false,
                Message = $"Failed to create user: {errors}"
            });
        }

        // Assign Member role
        await userManager.AddToRoleAsync(user, RoleConstants.Member);

        // Create Member profile
        var member = new Member
        {
            UserId = user.Id,
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Email = request.Email,
            Address = null,
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)), // Default to 18 years ago
            HasMedicalConditions = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();

        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "User registered successfully.",
            UserId = user.Id,
            Email = user.Email
        });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    public static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Results.BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Account is locked due to multiple failed login attempts."
                });
            }

            return Results.Unauthorized();
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user, roles, configuration);

        // Sign in with cookie (for browser-based clients)
        await signInManager.SignInAsync(user, isPersistent: true);

        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Logs out the current user
    /// </summary>
    public static async Task<IResult> LogoutAsync(SignInManager<IdentityUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "Logged out successfully."
        });
    }

    /// <summary>
    /// Gets the current authenticated user's information
    /// </summary>
    public static async Task<IResult> GetCurrentUserAsync(
        HttpContext httpContext,
        UserManager<IdentityUser> userManager)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);

        return Results.Ok(new AuthResponse
        {
            Success = true,
            Message = "User retrieved successfully.",
            UserId = user.Id,
            Email = user.Email,
            Roles = roles.ToList()
        });
    }

    private static string GenerateJwtToken(IdentityUser user, IList<string> roles, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "your-super-secret-key-change-this-in-production-min-32-chars!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "GymAppV3";
        var jwtAudience = configuration["Jwt:Audience"] ?? "GymAppV3";
        var jwtExpiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
