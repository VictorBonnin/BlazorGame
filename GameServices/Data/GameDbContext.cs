using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;   // <-- IMPORTANT

namespace GameServices.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Adventure> Adventures => Set<Adventure>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Player>()
            .HasIndex(p => p.UserName)
            .IsUnique();

        // Rooms (RoomPlay) stockées comme "owned collection"
        b.Entity<Adventure>()
         .OwnsMany(a => a.Rooms, rb =>
         {
             rb.WithOwner().HasForeignKey("AdventureId");
             rb.Property<int>("Id");
             rb.HasKey("Id");
         });
    }
}
