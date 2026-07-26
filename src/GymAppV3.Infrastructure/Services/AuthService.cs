using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymAppV3.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context,
        IDateTimeProvider clock,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _clock = clock;
        _configuration = configuration;
    }

    public async Task<RegisterResult> RegisterAsync(
        RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            return new RegisterEmailInUse();

        // User + Member creation is transactional: either both land or neither.
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var user = new IdentityUser
        {
            UserName = command.Email,
            Email = command.Email,
            EmailConfirmed = true       // TODO: flip to false when email verification lands
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new RegisterFailed(createResult.Errors.Select(e => e.Description).ToList());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Member);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new RegisterFailed(roleResult.Errors.Select(e => e.Description).ToList());
        }

        var member = new Member
        {
            UserId = user.Id,
            Firstname = command.Firstname,
            Lastname = command.Lastname,
            Email = command.Email,
            Phone = command.Phone,
            Address = new Address
            {
                Street = command.Address.Street,
                City = command.Address.City,
                State = command.Address.State,
                ZipCode = command.Address.ZipCode,
                Country = command.Address.Country
            },
            BirthDate = command.BirthDate,
            HasMedicalConditions = command.HasMedicalConditions,
            MedicalNotes = command.MedicalNotes
            // CreatedAt / CreatedBy are set by AuditableEntityInterceptor.
            // CreatedBy will be null since the request is anonymous — that's accurate.
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new RegisterSuccess(user.Id, user.Email!);
    }

    public async Task<LoginResult> LoginAsync(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return new LoginInvalidCredentials();

        // CheckPasswordSignInAsync does the hash check + lockout accounting,
        // without setting any cookie (SignInAsync is the cookie-setting one).
        var result = await _signInManager.CheckPasswordSignInAsync(
            user, command.Password, lockoutOnFailure: true);

        if (result.IsLockedOut) return new LoginLockedOut();
        if (!result.Succeeded) return new LoginInvalidCredentials();

        var roles = await _userManager.GetRolesAsync(user);
        var rolesList = roles.ToList();
        var token = GenerateJwtToken(user, roles);

        return new LoginSuccess(token, user.Id, user.Email!, rolesList);
    }

    public async Task<CurrentUserInfo?> GetCurrentUserAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserInfo(user.Id, user.Email!, roles.ToList());
    }

    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var jwtExpiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: _clock.UtcNow.UtcDateTime.AddMinutes(jwtExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
