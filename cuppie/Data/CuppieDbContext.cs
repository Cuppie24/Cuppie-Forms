using Microsoft.EntityFrameworkCore;
using cuppie.Models;

namespace cuppie.Data
{
    public class CuppieDbContext : DbContext
    {
        public CuppieDbContext(DbContextOptions<CuppieDbContext> options)
            : base(options)
        {
        }

        public DbSet<cuppie.Models.User> User { get; set; } = default!;
        public DbSet<cuppie.Models.Organisation> Organisation { get; set; } = default;
        public DbSet<cuppie.Models.UserOrganisation> UserOrganisation { get; set; } = default;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserOrganisation>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserOrganisations)
                .HasForeignKey(uo => uo.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserOrganisation>()
                .HasOne(uo => uo.Organisation)
                .WithMany(o => o.UserOrganisations)
                .HasForeignKey(uo => uo.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
