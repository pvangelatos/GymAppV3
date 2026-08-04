using GymWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IRecaptchaService _recaptchaService;

    public string RecaptchaSiteKey { get; }

    public LoginModel(
        SignInManager<IdentityUser> signInManager,
        ILogger<LoginModel> logger,
        IRecaptchaService recaptchaService,
        IConfiguration configuration)
    {
        _signInManager = signInManager;
        _logger = logger;
        _recaptchaService = recaptchaService;
        RecaptchaSiteKey = configuration["Recaptcha:SiteKey"]
            ?? throw new InvalidOperationException("Recaptcha:SiteKey is not configured.");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public string? RecaptchaToken { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var isHuman = await _recaptchaService.VerifyAsync(RecaptchaToken, "login", cancellationToken);
            if (!isHuman)
            {
                ModelState.AddModelError(string.Empty, "We couldn't verify you're not a robot. Please try again.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        return Page();
    }
}
