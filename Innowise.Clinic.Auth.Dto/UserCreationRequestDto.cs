using System.ComponentModel.DataAnnotations;

namespace Innowise.Clinic.Auth.Dto;

public class UserCreationRequestDto
{
    public UserCreationRequestDto(Guid entityId, string role, string email)
    {
        EntityId = entityId;
        Role = role;
        Email = email;
    }

    public Guid EntityId { get; }
    public string Role { get; }
    [EmailAddress] public string Email { get; }
}