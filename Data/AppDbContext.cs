using EventManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EventManagement.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser> {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        builder.Entity<Registration>().HasIndex(r => new { r.EventId, r.UserId }).IsUnique();
        builder.Entity<Event>().HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Registration>().HasOne(r => r.Event).WithMany(e => e.Registrations).HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Registration>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
