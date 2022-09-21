using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace VinOrgCLI.Validation;
internal class IsValidPackNameAttribute :ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        Regex regex = new Regex("[a-zA-Z0-9]");
        if(value is string packName)
        {
            if (regex.IsMatch(packName) && packName.Length<=15) return ValidationResult.Success;
        }
        return new ValidationResult("Invalid Pack Name");
    }
}
