namespace Innowise.Clinic.Auth.Persistence.Models;

public class DoctorInfo
{
    public Guid WorkerInfoId { get; set; }
    public Guid ProfileId { get; set; }
    public bool IsProfileActive { get; set; }
}