namespace Innowise.Clinic.Auth.Dto.RabbitMq;

[Serializable]
public record AccountStatusChangeDto(Guid AccountId, bool IsActiveStatus);