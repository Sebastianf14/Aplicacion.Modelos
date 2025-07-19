using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aplicacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }


        // POST: api/Users/5/favorites/artists/10
        [HttpPost("{userId}/favorites/artists/{artistId}")]
        public async Task<IActionResult> AddArtistToFavorites(int userId, int artistId)
        {
            var existingFavorite = await _context.FavoriteArtists
                .AnyAsync(fa => fa.UserId == userId && fa.ArtistId == artistId);

            if (existingFavorite)
                return BadRequest("Artist already in favorites");

            var favorite = new FavoritoArtista
            {
                UserId = userId,
                ArtistId = artistId,
                AddedDate = DateTime.Now
            };

            _context.FavoriteArtists.Add(favorite);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // DELETE: api/Users/5/favorites/artists/10
        [HttpDelete("{userId}/favorites/artists/{artistId}")]
        public async Task<IActionResult> RemoveArtistFromFavorites(int userId, int artistId)
        {
            var favorite = await _context.FavoriteArtists
                .FirstOrDefaultAsync(fa => fa.UserId == userId && fa.ArtistId == artistId);

            if (favorite == null)
                return NotFound();

            _context.FavoriteArtists.Remove(favorite);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Users/5/favorites/artists
        [HttpGet("{userId}/favorites/artists")]
        public async Task<ActionResult<IEnumerable<User>>> GetFavoriteArtists(int userId)
        {
            var favoriteArtists = await _context.FavoriteArtists
                .Where(fa => fa.UserId == userId)
                .Include(fa => fa.Artist)
                .OrderByDescending(fa => fa.AddedDate)
                .Select(fa => fa.Artist)
                .ToListAsync();

            return Ok(favoriteArtists);
        }
    }
}
