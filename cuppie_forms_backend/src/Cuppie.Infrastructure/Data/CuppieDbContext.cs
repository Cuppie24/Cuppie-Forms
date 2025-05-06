using Microsoft.EntityFrameworkCore;
using Cuppie.Domain.Entities;
namespace Cuppie.Infrastructure.Data
{
    public class CuppieDbContext (DbContextOptions<CuppieDbContext> options) : DbContext (options)
    {
        public DbSet<UserEntity> User { get; set; } = null!;
        public DbSet<OrganisationEntity>? Organisation { get; set; } = null;
        public DbSet<UserOrganisationEntity>? UserOrganisation { get; set; } = null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().ToTable("User");
            modelBuilder.Entity<OrganisationEntity>().ToTable("Organisation");
            modelBuilder.Entity<UserOrganisationEntity>().ToTable("UserOrganisation");

            modelBuilder.Entity<UserOrganisationEntity>()
                .HasOne(uo => uo.UserEntity)
                .WithMany(u => u.UserOrganisations)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserOrganisationEntity>()
                .HasOne(uo => uo.OrganisationEntity)
                .WithMany(o => o.UserOrganisations)
                .HasForeignKey(uo => uo.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
