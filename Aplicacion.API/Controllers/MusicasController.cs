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
    public class MusicasController : Controller
    {
        private readonly AppDbContext _context;

        public MusicasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Musics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Musica>>> GetMusic()
        {
            return await _context.Musics
                .Include(m => m.Artist)
                .Include(m => m.Album)
                .Include(m => m.PlaylistMusics)
                .ToListAsync();
        }

        // GET: api/Musics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Musica>> GetMusic(int id)
        {
            var music = await _context.Musics
                .Include(m => m.Artist)
                .Include(m => m.Album)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (music == null)
            {
                return NotFound();
            }

            return music;
        }

        // GET: api/MusicsByArtist/5
        [HttpGet("artist/{artistId}")]
        public async Task<ActionResult<IEnumerable<Musica>>> GetMusicsByArtist(int artistId)
        {
            var musics = await _context.Musics
                .Where(m => m.ArtistId == artistId)
                .Include(m => m.Artist)
                .Include(m => m.Album)
                .ToListAsync();

            return Ok(musics);
        }

        // PUT: api/Musics/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMusic(int id, Musica music)
        {
            if (id != music.Id)
            {
                return BadRequest();
            }

            _context.Entry(music).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MusicExists(id))
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

        // POST: api/Musics
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Musica>> PostMusic(Musica music)
        {
            _context.Musics.Add(music);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMusic", new { id = music.Id }, music);
        }

        // DELETE: api/Musics/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMusic(int id)
        {
            var music = await _context.Musics.FindAsync(id);
            if (music == null)
            {
                return NotFound();
            }

            _context.Musics.Remove(music);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MusicExists(int id)
        {
            return _context.Musics.Any(e => e.Id == id);
        }



        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Musica>>> SearchMusics(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query cannot be empty");
            }

            var musics = await _context.Musics
                .Include(m => m.Artist)
                .Where(m =>
                    m.Title.Contains(query) ||
                    (m.Artist.FirstName + " " + m.Artist.LastName).Contains(query) ||
                    m.Artist.FirstName.Contains(query) ||
                    m.Artist.LastName.Contains(query)
                )
                .ToListAsync();

            return Ok(musics);
        }
    }
}
