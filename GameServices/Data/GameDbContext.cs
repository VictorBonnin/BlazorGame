using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace GameServices.Data;

public class GameDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Adventure> Adventures { get; set; }

    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adventure>().HasKey(a => a.Id);
        modelBuilder.Entity<Player>().HasKey(p => p.Id);

        // Configuration de la relation One-to-Many entre Player et Adventure
        modelBuilder.Entity<Adventure>()
            .HasOne(a => a.Player)
            .WithMany()
            .HasForeignKey(a => a.PlayerId);

        // Configuration de la collection détenue (Owned Collection)
        modelBuilder.Entity<Adventure>().OwnsMany(
            a => a.Rooms,
            r =>
            {
                r.WithOwner().HasForeignKey("AdventureId");
                r.Property(rp => rp.Index);
                r.Property(rp => rp.Type);
                r.Property(rp => rp.Action);
                r.Property(rp => rp.Points);
                r.Property(rp => rp.Difficulty);
                r.HasKey(nameof(RoomPlay.Id), "AdventureId");
            });
    }
}