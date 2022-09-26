namespace VinOrgCLI.Validation;
internal class PathValidAttribute :ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if(value is string path)
        {
			bool isValid = Directory.Exists(path);
            if(isValid) return ValidationResult.Success;
        }
        return new ValidationResult("The provided path doesn't exist or invalid.");
    }
}
