using Microsoft.AspNetCore.Mvc;

namespace cuppie.Services
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {                
        private static List<string> users = new() { "Cuppie", "Frank" };

        [HttpGet]
        public IActionResult GetUsers() => Ok(users);

        [HttpPost]
        public IActionResult AddUser([FromBody] string todo)
        {
            users.Add(todo);
            return Ok(todo);
        }

        [HttpDelete("{index}")]
        public IActionResult DeleteUser(int index)
        {
            if (index < 0 || index >= users.Count)
                return NotFound("Элемент не найден");

            users.RemoveAt(index);
            return Ok("Удалено");
        }
    }
}
