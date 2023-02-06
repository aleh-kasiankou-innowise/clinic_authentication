using System.ComponentModel.DataAnnotations;

namespace Innowise.Clinic.Auth.Dto;

/// <summary>
/// Patient e-mail and password.
/// </summary>
public class PatientCredentialsDto
{
    /// <summary>
    /// Patient's e-mail address.
    /// </summary>
    /// <example>patient@gmail.com</example>
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    
    /// <summary>
    /// Patient's password.
    /// </summary>
    /// <example>lEm0nbangpuss</example>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)] 
    public string Password { get; set; }
}