using System.ComponentModel.DataAnnotations;

namespace BookmarkManager.Api.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class TrimmedNonWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is string s && string.IsNullOrWhiteSpace(s))
            return new ValidationResult($"The {ctx.DisplayName} field must not be whitespace-only.");
        return ValidationResult.Success;
    }
}
