namespace Innowise.Clinic.Auth.Validators.Interfaces;

public interface ITokenStateValidator
{
    Task<bool> IsTokenStateValid(Guid id);
}