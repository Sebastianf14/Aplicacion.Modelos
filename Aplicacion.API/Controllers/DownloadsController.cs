using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aplicacion.Modelos.Implementations;

namespace Aplicacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadsController : Controller
    {
        private readonly AppDbContext _context;

        public DownloadsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Downloads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Download>>> GetDownload()
        {
            return await _context.Downloads
                .Include(d => d.User)
                .Include(d => d.Music)
                    .ThenInclude(m => m.Artist)
                .ToListAsync();
        }

        // GET: api/Downloads/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Download>>> GetDownloadsByUser(int userId)
        {
            return await _context.Downloads
                .Where(d => d.UserId == userId)
                .Include(d => d.Music)
                    .ThenInclude(m => m.Artist)
                .OrderByDescending(d => d.DownloadDate)
                .ToListAsync();
        }
        // GET: api/Downloads/music/5 
        [HttpGet("music/{musicId}")]
        public async Task<ActionResult<IEnumerable<Download>>> GetDownloadsByMusic(int musicId)
        {
            return await _context.Downloads
                .Where(d => d.MusicId == musicId)
                .Include(d => d.User)
                .ToListAsync();
        }

        // GET: api/Downloads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Download>> GetDownload(int id)
        {
            var download = await _context.Downloads.FindAsync(id);

            if (download == null)
            {
                return NotFound();
            }

            return download;
        }

        // PUT: api/Downloads/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDownload(int id, Download download)
        {
            if (id != download.Id)
            {
                return BadRequest();
            }

            _context.Entry(download).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DownloadExists(id))
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

        // POST: api/Downloads
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Download>> PostDownload(Download download)
        {
            _context.Downloads.Add(download);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDownload", new { id = download.Id }, download);
        }

        // DELETE: api/Downloads/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDownload(int id)
        {
            var download = await _context.Downloads.FindAsync(id);
            if (download == null)
            {
                return NotFound();
            }

            _context.Downloads.Remove(download);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DownloadExists(int id)
        {
            return _context.Downloads.Any(e => e.Id == id);
        }
    }
}
