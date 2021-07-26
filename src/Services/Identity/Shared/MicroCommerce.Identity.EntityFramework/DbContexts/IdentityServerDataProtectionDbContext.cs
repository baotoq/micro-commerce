using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroCommerce.Identity.Admin.EntityFramework.Shared.DbContexts
{
    public class IdentityServerDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public IdentityServerDataProtectionDbContext(DbContextOptions<IdentityServerDataProtectionDbContext> options)
            : base(options) { }
    }
}
