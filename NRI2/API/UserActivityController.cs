using Microsoft.AspNetCore.Mvc;
using NRI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserActivityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateActivity([FromBody] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

}
