using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRI.Shared;


namespace NRI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _onlineThreshold = TimeSpan.FromMinutes(5);

        public UserActivityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateActivity([FromBody] UserActivityRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound();

            user.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<UserStatusDto>>> GetActiveUsers()
        {
            var threshold = DateTime.UtcNow - _onlineThreshold;

            var activeUsers = await _context.Users
                .Where(u => u.LastActivity >= threshold)
                .Select(u => new UserStatusDto
                {
                    UserId = u.Id,
                    Username = u.UserName,
                    LastActivity = u.LastActivity,
                    IsOnline = true
                })
                .ToListAsync();

            return Ok(activeUsers);
        }
    }

    public class UserActivityRequest
    {
        public int UserId { get; set; }
    }
}
