using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aplicacion.Modelos.Favorite;

namespace Aplicacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritoMusicasController : Controller
    {
        private readonly AppDbContext _context;

        public FavoritoMusicasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FavoriteMusics/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoritoMusica>>> GetFavoriteMusicsByUser(int userId)
        {
            return await _context.FavoriteMusics
                .Where(fm => fm.UserId == userId)
                .Include(fm => fm.Music)
                    .ThenInclude(m => m.Artist)
                .Include(fm => fm.Music)
                    .ThenInclude(m => m.Album)
                .OrderByDescending(fm => fm.AddedDate)
                .ToListAsync();
        }

        // POST: api/FavoriteMusics
        [HttpPost]
        public async Task<ActionResult<FavoritoMusica>> PostFavoriteMusic(FavoritoMusica favoriteMusic)
        {
            // Verificar si ya existe
            var existing = await _context.FavoriteMusics
                .AnyAsync(fm => fm.UserId == favoriteMusic.UserId && fm.MusicId == favoriteMusic.MusicId);

            if (existing)
                return BadRequest("Music already in favorites");

            favoriteMusic.AddedDate = DateTime.Now;
            _context.FavoriteMusics.Add(favoriteMusic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFavoriteMusic", new { id = favoriteMusic.Id }, favoriteMusic);
        }

        // DELETE: api/FavoriteMusics/user/5/music/10
        [HttpDelete("user/{userId}/music/{musicId}")]
        public async Task<IActionResult> DeleteFavoriteMusic(int userId, int musicId)
        {
            var favoriteMusic = await _context.FavoriteMusics
                .FirstOrDefaultAsync(fm => fm.UserId == userId && fm.MusicId == musicId);

            if (favoriteMusic == null)
                return NotFound();

            _context.FavoriteMusics.Remove(favoriteMusic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
