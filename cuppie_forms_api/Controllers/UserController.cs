using cuppie_forms_api.Data;
using cuppie_forms_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cuppie_forms_api.Controllers
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
