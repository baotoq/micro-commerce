using Identity.API.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>().HasData(
                new Role { Id = 1L, ConcurrencyStamp = "4776a1b2-dbe4-4056-82ec-8bed211d1454", Name = "admin", NormalizedName = "ADMIN" },
                new Role { Id = 2L, ConcurrencyStamp = "00d172be-03a0-4856-8b12-26d63fcf4374", Name = "customer", NormalizedName = "CUSTOMER" },
                new Role { Id = 3L, ConcurrencyStamp = "d4754388-8355-4018-b728-218018836817", Name = "guest", NormalizedName = "GUEST" },
                new Role { Id = 4L, ConcurrencyStamp = "71f10604-8c4d-4a7d-ac4a-ffefb11cefeb", Name = "vendor", NormalizedName = "VENDOR" }
            );

            builder.Entity<User>().HasData(
                new User { Id = 1L, AccessFailedCount = 0, ConcurrencyStamp = "c83afcbc-312c-4589-bad7-8686bd4754c0", Email = "admin@gmail.com", EmailConfirmed = false, LockoutEnabled = false, NormalizedEmail = "ADMIN@SIMPLCOMMERCE.COM", NormalizedUserName = "ADMIN@GMAIL.COM", PasswordHash = "AQAAAAEAACcQAAAAEAEqSCV8Bpg69irmeg8N86U503jGEAYf75fBuzvL00/mr/FGEsiUqfR0rWBbBUwqtw==", PhoneNumberConfirmed = false, SecurityStamp = "d6847450-47f0-4c7a-9fed-0c66234bf61f", TwoFactorEnabled = false, UserName = "admin@gmail.com" },
                new User { Id = 2L, AccessFailedCount = 0, ConcurrencyStamp = "101cd6ae-a8ef-4a37-97fd-04ac2dd630e4", Email = "bshop@gmail.com", EmailConfirmed = false, LockoutEnabled = false, NormalizedEmail = "BSHOP@GMAIL.COM", NormalizedUserName = "SYSTEM@SIMPLCOMMERCE.COM", PasswordHash = "AQAAAAEAACcQAAAAEAEqSCV8Bpg69irmeg8N86U503jGEAYf75fBuzvL00/mr/FGEsiUqfR0rWBbBUwqtw==", PhoneNumberConfirmed = false, SecurityStamp = "a9565acb-cee6-425f-9833-419a793f5fba", TwoFactorEnabled = false, UserName = "bshop@gmail.com" }
            );

            builder.Entity<IdentityUserRole<long>>().HasData(new IdentityUserRole<long> { UserId = 1L, RoleId = 1L });
            builder.Entity<IdentityUserRole<long>>().HasData(new IdentityUserRole<long> { UserId = 2L, RoleId = 2L });
        }
    }
}
