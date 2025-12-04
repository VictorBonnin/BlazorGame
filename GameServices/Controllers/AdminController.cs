using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServices.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text; // Nécessaire pour l'export CSV

namespace GameServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")] // S'assure que seul l'admin y accède
    public class AdminController : ControllerBase
    {
        private readonly GameDbContext _context;

        public AdminController(GameDbContext context)
        {
            _context = context;
        }

        // --- 1. Statistiques Globales (Déjà présent) ---
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

        // --- 2. Liste des joueurs pour le tableau de bord (C'était le MANQUANT) ---
        // Route : GET api/admin/players
        [HttpGet("players")]
        public async Task<ActionResult<IEnumerable<object>>> GetAdminPlayers()
        {
            // On récupère les joueurs avec leurs aventures pour calculer les stats
            var players = await _context.Players
                .Include(p => p.Adventures)
                .Select(p => new
                {
                    p.Id,
                    p.UserName,
                    p.IsActive,
                    // Calculs à la volée pour le Dashboard
                    GamesPlayed = p.Adventures.Count,
                    HighScore = p.Adventures.Any() ? p.Adventures.Max(a => a.Score) : 0
                })
                .ToListAsync();

            return Ok(players);
        }

        // --- 3. Activer / Désactiver un joueur ---
        // Route : PUT api/admin/players/{id}/toggle-status
        [HttpPut("players/{id}/toggle-status")]
        public async Task<IActionResult> TogglePlayerStatus(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            player.IsActive = !player.IsActive; // On inverse le statut
            await _context.SaveChangesAsync();

            return Ok();
        }

        // --- 4. Export CSV ---
        // Route : GET api/admin/players/export
        [HttpGet("players/export")]
        public async Task<IActionResult> ExportPlayersCsv()
        {
            var players = await _context.Players
                .Include(p => p.Adventures)
                .ToListAsync();

            var csv = new StringBuilder();
            // En-tête du CSV
            csv.AppendLine("Id,UserName,IsActive,GamesPlayed,HighScore,CreatedAt");

            foreach (var p in players)
            {
                var gamesPlayed = p.Adventures.Count;
                var highScore = p.Adventures.Any() ? p.Adventures.Max(a => a.Score) : 0;
                
                // Ligne de données
                csv.AppendLine($"{p.Id},{p.UserName},{p.IsActive},{gamesPlayed},{highScore},{p.CreatedAt}");
            }

            // Retourne le fichier CSV
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "players_export.csv");
        }
        
        // --- 5. Reset Data (Déjà présent) ---
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