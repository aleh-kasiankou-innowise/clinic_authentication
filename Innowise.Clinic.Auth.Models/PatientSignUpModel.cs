using System.ComponentModel.DataAnnotations;

namespace Innowise.Clinic.Auth.Models;

public class PatientSignUpModel
{
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)] 
    public string Password { get; set; }
}