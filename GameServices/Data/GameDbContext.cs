using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels; // <--- CORRECTION CS0103: Ajout pour RoomPlay

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

        // --- CORRECTION ICI ---
        modelBuilder.Entity<Adventure>()
            .HasOne(a => a.Player)
            .WithMany(p => p.Adventures) // <--- AJOUTE CECI : on lie la liste du Player
            .HasForeignKey(a => a.PlayerId);

        // Configuration de la collection détenue (inchangée)
        modelBuilder.Entity<Adventure>().OwnsMany(
            a => a.Rooms,
            r =>
            {
                r.WithOwner().HasForeignKey("AdventureId");
                r.Property(rp => rp.Index);
                // ... reste inchangé
                r.HasKey(nameof(RoomPlay.Id), "AdventureId");
            });
    }
}