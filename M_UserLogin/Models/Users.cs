using Microsoft.AspNetCore.Identity;

namespace M_UserLogin.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        // 🔐 Secret Code (like a 4-digit PIN or keyword for password recovery)
        // It’s required when the user forgets their password.
        public string SecretCode { get; set; } = string.Empty;
    }
}
