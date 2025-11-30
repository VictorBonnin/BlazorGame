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

        // GET: api/Players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        // GET: api/Players/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer(string id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

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