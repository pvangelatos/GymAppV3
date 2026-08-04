using FluentValidation;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Members;
using GymWebApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GymWebApp.Pages.Members;

[Authorize(Policy = "MemberOnly")]
public class EditProfileModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;
    private readonly IMemberCommandService _memberCommandService;
    private readonly IValidator<UpdateMemberCommand> _validator;


    public EditProfileModel(
        IMemberQueryService memberQueryService,
        IMemberCommandService memberCommandService,
        IValidator<UpdateMemberCommand> validator)
    {
        _memberQueryService = memberQueryService;
        _memberCommandService = memberCommandService;
        _validator = validator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Guid MemberId { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateOnly BirthDate { get; set; }

        [Display(Name = "I have medical conditions that may affect my training")]
        public bool HasMedicalConditions { get; set; }

        [StringLength(500)]
        [Display(Name = "Medical Conditions Details (optional)")]
        public string? MedicalConditionsDescription { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var memberDto = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        if (memberDto == null)
        {
            return RedirectToPage("/Members/CompleteProfile");
        }

        // Get detailed version
        var member = await _memberQueryService.GetByIdAsync(
            new GetMemberByIdQuery(memberDto.Id),
            cancellationToken);

        if (member == null)
        {
            return RedirectToPage("/Members/CompleteProfile");
        }

        MemberId = member.Id;
        Input = new InputModel
        {
            Firstname = member.Firstname,
            Lastname = member.Lastname,
            Phone = member.Phone,
            Street = member.Address.Street,
            City = member.Address.City,
            State = member.Address.State,
            ZipCode = member.Address.ZipCode,
            Country = member.Address.Country,
            BirthDate = member.BirthDate,
            HasMedicalConditions = member.HasMedicalConditions,
            MedicalConditionsDescription = member.MedicalNotes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var memberDto = await _memberQueryService.GetByUserIdAsync(
            new GetMemberByUserIdQuery(userId),
            cancellationToken);

        if (memberDto == null)
        {
            return RedirectToPage("/Members/CompleteProfile");
        }

        MemberId = memberDto.Id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Get current member email
            var currentMember = await _memberQueryService.GetByIdAsync(
                new GetMemberByIdQuery(memberDto.Id),
                cancellationToken);

            if (currentMember == null)
            {
                return RedirectToPage("/Members/CompleteProfile");
            }

            var address = new AddressDto(
                Input.Street,
                Input.City,
                Input.State,
                Input.ZipCode,
                Input.Country
            );

            var command = new UpdateMemberCommand(
                Id: memberDto.Id,
                Firstname: Input.Firstname,
                Lastname: Input.Lastname,
                Email: currentMember.Email,
                Phone: Input.Phone,
                Address: address,
                BirthDate: Input.BirthDate,
                HasMedicalConditions: Input.HasMedicalConditions,
                MedicalNotes: Input.MedicalConditionsDescription
            );

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return Page();
            }

            await _memberCommandService.UpdateAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Your profile has been updated successfully!";
            return RedirectToPage("/Members/Profile");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
}
