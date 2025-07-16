using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.UserInput;

namespace AgentWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInputController : ControllerBase
    {
        private readonly IUserInputService _userInputService;

        public UserInputController(IUserInputService userInputService)
        {
            _userInputService = userInputService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessInput([FromBody] string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest("Input cannot be empty.");
            }

            var result = await _userInputService.ProcessUserInputAsync(input);
            return Ok(result);
        }
    }
}

