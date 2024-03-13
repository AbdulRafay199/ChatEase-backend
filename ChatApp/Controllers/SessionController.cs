using ChatApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SessionController(AppDbContext context) {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllSession()
        {
            return Ok(new { Sessions = _context.Sessions.ToList() });
        }

        [HttpDelete]
        public async Task<object> removeSession([FromBody] int id)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(e => e.UserId == id);
            if(session != null)
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                return Ok(new { msg = "Session removed" });
            }
            else
            {
                return NotFound(new { msg = "Session not found" });
            }
        }
    }
}
