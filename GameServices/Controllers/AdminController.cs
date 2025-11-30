using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServices.Data;
using Microsoft.AspNetCore.Authorization;

namespace GameServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly GameDbContext _context;

        public AdminController(GameDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetGlobalStats()
        {
            var playerCount = await _context.Players.CountAsync();
            var adventureCount = await _context.Adventures.CountAsync();
            
            var avgScore = adventureCount > 0 
                ? await _context.Adventures.AverageAsync(a => a.Score) 
                : 0;

            return Ok(new 
            { 
                TotalPlayers = playerCount, 
                TotalAdventures = adventureCount, 
                AverageScore = avgScore 
            });
        }
        
        [HttpDelete("reset-data")]
        public async Task<IActionResult> ResetData()
        {
            _context.Adventures.RemoveRange(_context.Adventures);
            _context.Players.RemoveRange(_context.Players);
            await _context.SaveChangesAsync();
            return Ok("Base de données réinitialisée.");
        }
    }
}