using Microsoft.AspNetCore.Identity;

namespace InventorySystem.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
