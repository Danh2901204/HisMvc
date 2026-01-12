using Microsoft.AspNetCore.Identity;

namespace HisMvc.Data;

public class AppUser : IdentityUser
{
    public int? StaffId { get; set; }
}
