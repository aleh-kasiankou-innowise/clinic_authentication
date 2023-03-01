using System.ComponentModel.DataAnnotations;

namespace Innowise.Clinic.Auth.Dto;

// TODO MOVE TO A SHARED NUGET PACKAGE
public class AccountGenerationRequestDto
{
    public AccountGenerationRequestDto(Guid entityId, string role, string email)
    {
        EntityId = entityId;
        Role = role;
        Email = email;
    }

    public Guid EntityId { get; }
    public string Role { get; }
    [EmailAddress] public string Email { get; }
}