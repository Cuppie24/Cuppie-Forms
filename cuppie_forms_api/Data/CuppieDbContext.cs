using Microsoft.EntityFrameworkCore;
using cuppie_forms_api.Models;

namespace cuppie_forms_api.Data
{
    public class CuppieDbContext (DbContextOptions<CuppieDbContext> options) : DbContext (options)
    {
        public DbSet<User> User { get; set; } = null!;
        public DbSet<Organisation>? Organisation { get; set; } = null;
        public DbSet<UserOrganisation>? UserOrganisation { get; set; } = null;

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
