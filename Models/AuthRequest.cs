namespace XAccess2
{
    public class AuthRequest
    {
        public string EmailAddress { get; set; }
        public string MFACode { get; set; }
        public string IPAddress { get; set; }
        public string DealNumber { get; set; }
    }
}
