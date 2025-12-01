using GameServices.Data;
using GameServices.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using SharedModels.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GameServices.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Player")]
public class AdventuresController : ControllerBase
{
    private readonly GameDbContext _db;

    public AdventuresController(GameDbContext db)
    {
        _db = db;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartPayload>> StartAdventure(StartAdventureDto dto)
    {
        // 1. On cherche le joueur
        var player = await _db.Players.FindAsync(dto.PlayerId);

        // 2. Si le joueur n'existe pas, on le crée (Compatible avec ta classe Player actuelle)
        if (player is null)
        {
            string playerName = User.Identity?.Name ?? "Aventurier Inconnu";

            player = new Player
            {
                Id = dto.PlayerId,
                UserName = playerName, // CORRECTION : 'Name' devient 'UserName'
                CreatedAt = DateTime.UtcNow,
                IsActive = true
                // SUPPRESSION : Level, Experience et Gold n'existent pas dans ton modèle Player
            };

            _db.Players.Add(player);
            await _db.SaveChangesAsync();
        }

        // 3. Création de l'aventure
        var adv = new Adventure { PlayerId = player.Id, CreatedAt = DateTime.UtcNow };
        _db.Adventures.Add(adv);
        await _db.SaveChangesAsync();

        // 4. Génération du donjon
        var rooms = DungeonGenerator.GenerateDungeon(dto.MinRooms ?? 3, dto.MaxRooms ?? 5);
        
        return Created($"/api/adventures/{adv.Id}", new StartPayload(adv, rooms));
    }

    [HttpPost("{id}/finish")]
    public async Task<ActionResult<Adventure>> FinishAdventure(int id, FinishAdventureDto dto)
    {
        var adv = await _db.Adventures.FirstOrDefaultAsync(a => a.Id == id);
        if (adv is null) return NotFound();

        adv.Score = dto.Score;
        adv.FinishedAt = DateTime.UtcNow;

        if (dto.Rooms != null)
        {
            adv.Rooms = dto.Rooms.Select(r => new RoomPlay
            {
                Index = r.Index,
                Type = (RoomType)r.Type,
                Difficulty = r.Difficulty,
                Action = (PlayerAction)r.Action,
                Points = r.Points
            }).ToList();
        }
        else
        {
            adv.Rooms = new List<RoomPlay>();
        }

        await _db.SaveChangesAsync();
        return Ok(adv);
    }
    
    [HttpGet("/api/leaderboard")]
    public async Task<ActionResult<List<Adventure>>> GetLeaderboard([FromQuery] int top = 10)
    {
        var data = await _db.Adventures
            .Include(a => a.Player)
            .Where(a => a.FinishedAt != null)
            .OrderByDescending(a => a.Score)
            .Take(top)
            .ToListAsync();

        return Ok(data);
    }
}

public record StartAdventureDto(int PlayerId, int? MinRooms, int? MaxRooms);
public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);
public record FinishAdventureDto(int Score, List<RoomPlayDto>? Rooms);
public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);