using Microsoft.AspNetCore.Identity;

namespace ArderBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Goal { get; set; }
        public DateTime? BirthDate { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? Experience { get; set; }
        public string? DiscoverySource { get; set; }
        public DateTime? TermsAcceptedDate { get; set; }
    }
}
