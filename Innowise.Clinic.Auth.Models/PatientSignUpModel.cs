using System.ComponentModel.DataAnnotations;

namespace Innowise.Clinic.Auth.Models;

public class PatientSignUpModel
{
    [EmailAddress] 
    public string Email { get; set; }
    public string Password { get; set; }
}