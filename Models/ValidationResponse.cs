namespace XAccess2
{
    public class ValidationResponse
    {
        public Guid Id { get; set; }
        public string? MailDomainName { get; set; }
        public bool? RequiresMFA { get; set; } = true;
        public bool? RequiresXNumber { get; set; } = false;
        public int? DefaultXNumber { get; set; }
        public bool? IsActive { get; set; } = false;
    }
}
