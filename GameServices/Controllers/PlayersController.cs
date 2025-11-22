using GameServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace GameServices.Controllers;

[ApiController]
[Route("api/[controller]")] // L'URL sera automatique : /api/players
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _db;

    public PlayersController(GameDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Player>>> GetPlayers()
    {
        return Ok(await _db.Players.OrderBy(p => p.Id).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var p = await _db.Players.Include(x => x.Adventures).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(PlayerCreateDto dto)
    {
        var p = new Player { UserName = dto.UserName.Trim() };
        _db.Players.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlayer), new { id = p.Id }, p);
    }
}

// Tu peux déplacer tes DTOs (Data Transfer Objects) dans un fichier à part ou en bas du contrôleur
public record PlayerCreateDto(string UserName);