namespace XAccess2
{
    public class AuthResponse
    {
        public Guid AuthId { get; set; }
        public string Email { get; set; }
        public string KeyHash { get; set; }
        public string TopSec { get; set; }
        public DateTime? SignUp { get; set; }
        public string XNumber { get; set; }
        public long? ContactId { get; set; }
    }
}
