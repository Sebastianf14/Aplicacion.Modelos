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
    // FavoriteArtistsController.cs en API
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritoArtistasController : Controller
    {

        private readonly AppDbContext _context;

        public FavoritoArtistasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FavoriteArtists/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoritoArtista>>> GetFavoriteArtistsByUser(int userId)
        {
            return await _context.FavoriteArtists
                .Where(fa => fa.UserId == userId)
                .Include(fa => fa.Artist)
                .OrderByDescending(fa => fa.AddedDate)
                .ToListAsync();
        }

        // POST: api/FavoriteArtists
        [HttpPost]
        public async Task<ActionResult<FavoritoArtista>> PostFavoriteArtist(FavoritoArtista favoriteArtist)
        {
            // Verificar si ya existe
            var existing = await _context.FavoriteArtists
                .AnyAsync(fa => fa.UserId == favoriteArtist.UserId && fa.ArtistId == favoriteArtist.ArtistId);

            if (existing)
                return BadRequest("Artist already in favorites");

            favoriteArtist.AddedDate = DateTime.Now;
            _context.FavoriteArtists.Add(favoriteArtist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFavoriteArtist", new { id = favoriteArtist.Id }, favoriteArtist);
        }

        // DELETE: api/FavoriteArtists/user/5/artist/10
        [HttpDelete("user/{userId}/artist/{artistId}")]
        public async Task<IActionResult> DeleteFavoriteArtist(int userId, int artistId)
        {
            var favoriteArtist = await _context.FavoriteArtists
                .FirstOrDefaultAsync(fa => fa.UserId == userId && fa.ArtistId == artistId);

            if (favoriteArtist == null)
                return NotFound();

            _context.FavoriteArtists.Remove(favoriteArtist);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
