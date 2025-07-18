using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.UserInput;
using AgentWebApi.Services.Prometheus;

namespace AgentWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInputController : ControllerBase
    {
        private readonly IUserInputService _userInputService;
        private readonly IPrometheusService _prometheusService;

        public UserInputController(IUserInputService userInputService, IPrometheusService prometheusService)
        {
            _userInputService = userInputService;
            _prometheusService = prometheusService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessInput([FromBody] string input)
        {
            _prometheusService.IncrementRequestCounter("UserInput/ProcessInput");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (string.IsNullOrEmpty(input))
            {
                return BadRequest("Input cannot be empty.");
            }

            var result = await _userInputService.ProcessUserInputAsync(input);

            stopwatch.Stop();
            _prometheusService.ObserveRequestDuration("UserInput/ProcessInput", stopwatch.Elapsed.TotalSeconds);

            return Ok(result);
        }
    }
}

