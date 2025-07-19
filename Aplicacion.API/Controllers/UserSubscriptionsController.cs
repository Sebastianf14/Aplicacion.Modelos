using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aplicacion.Modelos.Suscription;

namespace Aplicacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubscriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public UserSubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserSubscriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSubscription>>> GetUserSubscription()
        {
            return await _context.UserSubscriptions
                .Include(us => us.User)
                .Include(us => us.SubscriptionPlan)
                .ToListAsync();
        }

        // GET: api/UserSubscriptions/user/5 
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserSubscription>>> GetSubscriptionsByUser(int userId)
        {
            return await _context.UserSubscriptions
                .Where(us => us.UserId == userId)
                .Include(us => us.SubscriptionPlan)
                .OrderByDescending(us => us.StartDate)
                .ToListAsync();
        }



        // GET: api/UserSubscriptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserSubscription>> GetUserSubscription(int id)
        {
            var userSubscription = await _context.UserSubscriptions.FindAsync(id);

            if (userSubscription == null)
            {
                return NotFound();
            }

            return userSubscription;
        }

        // PUT: api/UserSubscriptions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserSubscription(int id, UserSubscription userSubscription)
        {
            if (id != userSubscription.Id)
            {
                return BadRequest();
            }

            _context.Entry(userSubscription).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserSubscriptionExists(id))
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

        // POST: api/UserSubscriptions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserSubscription>> PostUserSubscription(UserSubscription userSubscription)
        {
            _context.UserSubscriptions.Add(userSubscription);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserSubscription", new { id = userSubscription.Id }, userSubscription);
        }

        // DELETE: api/UserSubscriptions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserSubscription(int id)
        {
            var userSubscription = await _context.UserSubscriptions.FindAsync(id);
            if (userSubscription == null)
            {
                return NotFound();
            }

            _context.UserSubscriptions.Remove(userSubscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserSubscriptionExists(int id)
        {
            return _context.UserSubscriptions.Any(e => e.Id == id);
        }

        // GET: api/UserSubscriptions/user/5/active
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<UserSubscription>> GetActiveSubscription(int userId)
        {
            var subscription = await _context.UserSubscriptions
                .Where(us => us.UserId == userId && us.IsActive && us.EndDate > DateTime.Now)
                .Include(us => us.SubscriptionPlan)
                .Include(us => us.User)
                .OrderByDescending(us => us.StartDate)
                .FirstOrDefaultAsync();

            if (subscription == null)
                return NotFound("No active subscription found");

            return subscription;
        }

        // POST: api/UserSubscriptions/5/cancel
        [HttpPost("{subscriptionId}/cancel")]
        public async Task<IActionResult> CancelSubscription(int subscriptionId)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(subscriptionId);
            if (subscription == null)
                return NotFound();

            subscription.IsActive = false;
            subscription.Status = UserSubscription.SubscriptionStatus.Cancelled;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/UserSubscriptions/5/renew
        [HttpPost("{subscriptionId}/renew")]
        public async Task<IActionResult> RenewSubscription(int subscriptionId, [FromBody] int months = 1)
        {
            var subscription = await _context.UserSubscriptions
                .Include(us => us.SubscriptionPlan)
                .FirstOrDefaultAsync(us => us.Id == subscriptionId);

            if (subscription == null)
                return NotFound();

            // Extender la suscripción
            subscription.EndDate = subscription.EndDate.AddMonths(months);
            subscription.IsActive = true;
            subscription.Status = UserSubscription.SubscriptionStatus.Active;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
