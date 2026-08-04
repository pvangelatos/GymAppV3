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

namespace GymWebApp.Pages.Staff.Members;

[Authorize(Policy = "StaffOnly")]
public class EditModel : PageModel
{
    private readonly IMemberQueryService _memberQueryService;
    private readonly IMemberCommandService _memberCommandService;
    private readonly IValidator<UpdateMemberCommand> _validator;


    public EditModel(
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
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateOnly BirthDate { get; set; }

        public bool HasMedicalConditions { get; set; }

        [StringLength(500)]
        public string? MedicalNotes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var member = await _memberQueryService.GetByIdAsync(
            new GetMemberByIdQuery(id),
            cancellationToken);

        if (member == null)
        {
            return NotFound();
        }

        MemberId = member.Id;
        Input = new InputModel
        {
            Firstname = member.Firstname,
            Lastname = member.Lastname,
            Email = member.Email,
            Phone = member.Phone,
            Street = member.Address.Street,
            City = member.Address.City,
            State = member.Address.State,
            ZipCode = member.Address.ZipCode,
            Country = member.Address.Country,
            BirthDate = member.BirthDate,
            HasMedicalConditions = member.HasMedicalConditions,
            MedicalNotes = member.MedicalNotes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        MemberId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var address = new AddressDto(
                Input.Street,
                Input.City,
                Input.State,
                Input.ZipCode,
                Input.Country
            );

            var command = new UpdateMemberCommand(
                Id: id,
                Firstname: Input.Firstname,
                Lastname: Input.Lastname,
                Email: Input.Email,
                Phone: Input.Phone,
                Address: address,
                BirthDate: Input.BirthDate,
                HasMedicalConditions: Input.HasMedicalConditions,
                MedicalNotes: Input.MedicalNotes
            );

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return Page();
            }

            await _memberCommandService.UpdateAsync(command, cancellationToken);

            TempData["SuccessMessage"] = "Member updated successfully!";
            return RedirectToPage("/Staff/Members/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
}
