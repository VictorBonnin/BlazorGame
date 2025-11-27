using GameServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GameServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly GameDbContext _db;

    public AdminController(GameDbContext db)
    {
        _db = db;
    }

    // 1. Liste des joueurs avec stats
    [HttpGet("players")]
    public async Task<ActionResult> GetPlayersList()
    {
        var players = await _db.Players
            .Include(p => p.Adventures)
            .Select(p => new 
            {
                p.Id,
                p.UserName,
                p.IsActive,
                GamesPlayed = p.Adventures.Count,
                HighScore = p.Adventures.Any() ? p.Adventures.Max(a => a.Score) : 0
            })
            .ToListAsync();
        return Ok(players);
    }

    // 2. Désactiver / Réactiver un joueur
    [HttpPut("players/{id}/toggle-status")]
    public async Task<ActionResult> TogglePlayerStatus(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.IsActive = !player.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { player.Id, player.IsActive });
    }

    // 3. Export des joueurs en CSV
    [HttpGet("players/export")]
    public async Task<IActionResult> ExportPlayersCsv()
    {
        var players = await _db.Players.ToListAsync();
        var builder = new StringBuilder();
        builder.AppendLine("Id,UserName,IsActive,CreatedAt");

        foreach (var p in players)
        {
            builder.AppendLine($"{p.Id},{p.UserName},{p.IsActive},{p.CreatedAt}");
        }

        return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "players_export.csv");
    }

    // 4. Liste globale des aventures (Historique global)
    [HttpGet("adventures")]
    public async Task<ActionResult> GetAllAdventures()
    {
        var adventures = await _db.Adventures
            .Include(a => a.Player)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100) // Limite pour la performance
            .ToListAsync();
        return Ok(adventures);
    }

    [HttpGet("metadata/rooms")]
    public ActionResult GetRoomTypes()
    {
        // Renvoie la liste des types de salles disponibles (basé sur ton Enum)
        var types = Enum.GetValues(typeof(SharedModels.RoomType))
                        .Cast<SharedModels.RoomType>()
                        .Select(t => t.ToString())
                        .ToList();
        return Ok(types);
    }
}