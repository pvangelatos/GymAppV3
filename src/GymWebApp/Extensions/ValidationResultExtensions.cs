using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GymWebApp.Extensions;

public static class ValidationResultExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error.ErrorMessage);
        }
    }
}