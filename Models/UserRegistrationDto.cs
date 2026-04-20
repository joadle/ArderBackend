namespace ArderBackend.Models
{
    public class UserRegistrationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        
        public string? Goal { get; set; }
        public DateTime? BirthDate { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? Experience { get; set; }
        public string? DiscoverySource { get; set; }
        public DateTime? TermsAcceptedDate { get; set; }
    }
}
