namespace XAccess2
{
    public class AuthStatusResponse
    {
        public string EmailAddress { get; set; }
        public string IPAddress { get; set; }
        public string MFACode { get; set; }
        public string DealNumber { get; set; }
        public bool IsValidated { get; set; } = false;
        public string RedirectURL { get; set; }
        public bool XNumberMatches { get; set; } = false;
    }
}
