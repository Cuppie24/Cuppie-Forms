using Microsoft.AspNetCore.Mvc;
using cuppie.Models;
using Microsoft.EntityFrameworkCore;
using cuppie.Data;
namespace cuppie.API
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly CuppieDbContext _dbContext;
        public UserController(CuppieDbContext context)
        {
            _dbContext = context;
        }
        [HttpGet("all")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _dbContext.User.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserData()
        {

            return Ok();
        }

    }    
}
