using GymAppV3.Infrastructure.Identity;
using GymWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IRecaptchaService _recaptchaService;

    public string RecaptchaSiteKey { get; }

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        IRecaptchaService recaptchaService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _recaptchaService = recaptchaService;
        RecaptchaSiteKey = configuration["Recaptcha:SiteKey"]
            ?? throw new InvalidOperationException("Recaptcha:SiteKey is not configured.");

    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public string? RecaptchaToken { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();

        var isHuman = await _recaptchaService.VerifyAsync(RecaptchaToken, "register", cancellationToken);
        if (!isHuman)
        {
            ModelState.AddModelError(string.Empty, "We couldn't verify you 're not a robot. Please try again.");
        }

        var user = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true // no email-confirmation flow yet
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        // Every self-service registration becomes a Member. Trainer/Admin accounts
        // are provisioned separately by an admin, not through this form.
        await _userManager.AddToRoleAsync(user, RoleConstants.Member);

        _logger.LogInformation(
            "New user {Email} registered and assigned {Role} role.",
            Input.Email, RoleConstants.Member);

        // Cookie sign-in so subsequent requests hit MemberOnly protected pages.
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Step 2 of the two-step registration: collect the Member profile fields.
        return RedirectToPage("/Members/CompleteProfile");
    }
}