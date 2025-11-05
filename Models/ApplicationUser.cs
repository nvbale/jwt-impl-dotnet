using Microsoft.AspNetCore.Identity;

namespace jwt_impl.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}