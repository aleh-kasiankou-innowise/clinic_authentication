namespace Innowise.Clinic.Auth.Services.UserManagementService.Data;

public class AuthenticationRequirementsSettings
{
    public bool ValidateUserEmailConfirmedOnLogin { get; set; } = true;
    public int MinimalPasswordLength { get; set; } = 6;
    public int MaximalPasswordLength { get; set; } = 15;
    public bool RequireLowercaseLettersInPassword { get; set; } = true;
    public bool RequireNonAlphanumericInPassword { get; set; } = true;
    public bool RequireUppercaseLettersInPassword { get; set; } = true;
}