using Microsoft.EntityFrameworkCore;
using Cuppie.Domain.Entities;
namespace Cuppie.Infrastructure.Data
{
    public class CuppieDbContext (DbContextOptions<CuppieDbContext> options) : DbContext (options)
    {
        public DbSet<UserEntity> User { get; set; } 
        public DbSet<OrganisationEntity> Organisation { get; set; } 
        public DbSet<UserOrganisationEntity> UserOrganisation { get; set; } 
        public DbSet<PageEntity> Page { get; set; } 
        public DbSet<WorkspaceEntity> Workspace { get; set; }
        public DbSet<RefreshTokenEntity> RefreshToken { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().ToTable("User");
            modelBuilder.Entity<OrganisationEntity>().ToTable("Organisation");
            modelBuilder.Entity<UserOrganisationEntity>().ToTable("UserOrganisation");
            modelBuilder.Entity<PageEntity>().ToTable("Page");
            modelBuilder.Entity<WorkspaceEntity>().ToTable("Workspace");
            modelBuilder.Entity<RefreshTokenEntity>().ToTable("RefreshToken");
            

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
            
            modelBuilder.Entity<PageEntity>()
                .HasOne(uo => uo.WorkspaceEntity)
                .WithMany(u => u.Pages)
                .HasForeignKey(uo => uo.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkspaceEntity>()
                .HasOne(uo => uo.OrganisationEntity)
                .WithMany(u => u.Workspaces)
                .HasForeignKey(uo => uo.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
