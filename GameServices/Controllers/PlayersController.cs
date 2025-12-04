using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServices.Data;
using SharedModels.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GameServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Player")]
    public class PlayersController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayersController(GameDbContext context)
        {
            _context = context;
        }

        // 👇 NOUVELLE ROUTE : Récupère le profil via le Token Keycloak
        [HttpGet("me")]
        public async Task<ActionResult<Player>> GetMyProfile()
        {
            // Le pseudo est extrait automatiquement du Token (grâce à ta config dans Program.cs)
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username)) 
                return Unauthorized();

            var player = await _context.Players
                .Include(p => p.Adventures) // On inclut les aventures pour les stats
                .ThenInclude(a => a.Rooms)  // On inclut les salles pour le détail
                .FirstOrDefaultAsync(p => p.UserName == username);

            if (player == null)
            {
                // Optionnel : Créer le joueur à la volée s'il n'existe pas encore
                player = new Player { UserName = username };
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
            }

            return player;
        }

        // GET: api/Players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        // GET: api/Players/5 (Correction du type string -> int)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.Adventures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return NotFound();

            return player;
        }

        // POST: api/Players
        [HttpPost]
        public async Task<ActionResult<Player>> PostPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }
    }
}