namespace Innowise.Clinic.Auth.Dto;

public class UserProfileLinkingDto
{
    public UserProfileLinkingDto(Guid userId, Guid profileId)
    {
        ProfileId = profileId;
        UserId = userId;
    }

    public Guid ProfileId { get; }
    public Guid UserId { get; }
}