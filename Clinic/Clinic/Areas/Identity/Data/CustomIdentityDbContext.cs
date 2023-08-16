using Clinic.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Areas.Identity.Data;

public class CustomIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public CustomIdentityDbContext(DbContextOptions<CustomIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        base.OnModelCreating(builder);

        Guid guid = Guid.NewGuid();
        string role_admin_guid = Guid.NewGuid().ToString();
        string user_admin_guid = Guid.NewGuid().ToString();
        string user_admin_security_stamp = Guid.NewGuid().ToString();
        string user_admin_concurency_stamp = Guid.NewGuid().ToString();

        builder.Entity<IdentityRole>()
            .HasData(new IdentityRole
            {
                Id = role_admin_guid,
                Name = "admin",
                NormalizedName = "admin",
                ConcurrencyStamp = role_admin_guid

            });
        var adminuser = new ApplicationUser
        {
            Id = user_admin_guid,
            UserName = "admin@simplex.ru",
            NormalizedUserName = "ADMIN@SIMPLEX.RU",
            Email = "admin@simplex48.ru",
            NormalizedEmail = "ADMIN@SIMPLEX48.RU",
            SecurityStamp = user_admin_security_stamp,
            ConcurrencyStamp = user_admin_concurency_stamp,

            LockoutEnabled = true,
            EmailConfirmed = true
        };
        adminuser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(adminuser, "qwerty");
        builder.Entity<ApplicationUser>()
            .HasData(adminuser
            );
        builder.Entity<IdentityUserRole<string>>()
            .HasData(new IdentityUserRole<string>
            {
                RoleId = role_admin_guid,
                UserId = user_admin_guid
            });
    }
}