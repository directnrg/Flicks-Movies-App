using _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _301153142_301137955_Soto_Ko_Lab3.Areas.Identity.Data;

public class CustomLab3Context : IdentityDbContext<CustomUser>
{
    public CustomLab3Context(DbContextOptions<CustomLab3Context> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
