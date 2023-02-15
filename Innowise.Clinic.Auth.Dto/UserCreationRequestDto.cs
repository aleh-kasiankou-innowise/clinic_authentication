namespace Innowise.Clinic.Auth.Dto;

public class UserCreationRequestDto
{
    public UserCreationRequestDto(Guid entityId, string role)
    {
        EntityId = entityId;
        Role = role;
    }

    public Guid EntityId { get; }
    public string Role { get; }
}