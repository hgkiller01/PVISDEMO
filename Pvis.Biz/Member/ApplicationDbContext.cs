using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Pvis.Biz.Member
{
    public class ApplicationDbContext : IdentityDbContext<MyAppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public virtual DbSet<ChangePwdHistory> ChangePwdHistory { get; set; }
        public virtual DbSet<LoginLog> LoginLog { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MyAppUser>(entity =>
            {

                entity.Property(e => e.Uid)
                    .UseIdentityColumn()
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                entity.Property(e => e.DisplayName)
                    .HasDefaultValue(String.Empty);

                entity.Property(e => e.AppPid)
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            });

            builder.Entity<ChangePwdHistory>(entity =>
            {

                entity.HasKey(e => e.Pid).IsClustered(false);

                entity.HasIndex(e => e.Id).IsClustered(true);

                entity.Property(e => e.PasswordHash).IsUnicode(false);

                entity.Property(e => e.LogDt).HasDefaultValueSql("(getdate())");

            });
        }
    }
}
