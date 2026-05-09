using Microsoft.AspNetCore.Identity;

namespace OrderSystem.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; } = "User"; // Simplified role tracking
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
