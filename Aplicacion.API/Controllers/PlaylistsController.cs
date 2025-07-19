using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aplicacion.Modelos;

namespace Aplicacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistsController : Controller
    {
        private readonly AppDbContext _context;

        public PlaylistsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Playlists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylist()
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistMusics)
                    .ThenInclude(pm => pm.Music)
                    .ThenInclude(pm => pm.Artist)
                .ToListAsync();
        }
        // GET: api/Playlists/user/5 
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Playlist>>> GetPlaylistsByUser(int userId)
        {
            return await _context.Playlists
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.PlaylistMusics)
                    .ThenInclude(pm => pm.Music)
                        .ThenInclude(m => m.Artist)
                .ToListAsync();
        }

        // GET: api/Playlists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Playlist>> GetPlaylist(int id)
        {
            var playlist = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistMusics.OrderBy(pm => pm.Order))
                    .ThenInclude(pm => pm.Music)
                        .ThenInclude(m => m.Artist)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
            {
                return NotFound();
            }

            return playlist;
        }

        // PUT: api/Playlists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlaylist(int id, Playlist playlist)
        {
            if (id != playlist.Id)
            {
                return BadRequest();
            }

            _context.Entry(playlist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaylistExists(id))
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

        // POST: api/Playlists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Playlist>> PostPlaylist(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlaylist", new { id = playlist.Id }, playlist);
        }

        // DELETE: api/Playlists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlaylistExists(int id)
        {
            return _context.Playlists.Any(e => e.Id == id);
        }

        // POST: api/Playlists/5/music/10
        [HttpPost("{playlistId}/music/{musicId}")]
        public async Task<IActionResult> AddMusicToPlaylist(int playlistId, int musicId)
        {
            // existan
            var playlist = await _context.Playlists.FindAsync(playlistId);
            var music = await _context.Musics.FindAsync(musicId);

            if (playlist == null || music == null)
                return NotFound();

            var existing = await _context.PlaylistMusics
                .AnyAsync(pm => pm.PlaylistId == playlistId && pm.MusicId == musicId);

            if (existing)
                return BadRequest("Music already in playlist");

            // Agregar
            var playlistMusic = new PlaylistMusica
            {
                PlaylistId = playlistId,
                MusicId = musicId,
                Order = await GetNextOrder(playlistId)
            };

            _context.PlaylistMusics.Add(playlistMusic);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Playlists/5/music/10 
        [HttpDelete("{playlistId}/music/{musicId}")]
        public async Task<IActionResult> RemoveMusicFromPlaylist(int playlistId, int musicId)
        {
            var playlistMusic = await _context.PlaylistMusics
                .FirstOrDefaultAsync(pm => pm.PlaylistId == playlistId && pm.MusicId == musicId);

            if (playlistMusic == null)
                return NotFound();

            _context.PlaylistMusics.Remove(playlistMusic);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<int> GetNextOrder(int playlistId)
        {
            var maxOrder = await _context.PlaylistMusics
                .Where(pm => pm.PlaylistId == playlistId)
                .MaxAsync(pm => (int?)pm.Order) ?? 0;
            return maxOrder + 1;
        }
    }
}
