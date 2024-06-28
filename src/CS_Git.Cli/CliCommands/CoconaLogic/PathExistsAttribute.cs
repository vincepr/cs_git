using System.ComponentModel.DataAnnotations;

namespace CS_Git.Cli.CliCommands.CoconaLogic;

/// <summary>
/// Enforce that path exists on the file system.
/// </summary>
class PathExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && (Directory.Exists(path) || File.Exists(path)))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult($"The path '{value}' is not found.");
    }
}