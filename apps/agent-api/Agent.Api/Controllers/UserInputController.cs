namespace Agent.Api.Controllers;

/// <summary>
/// User input controller
/// 用户输入控制器
/// 
/// 提供用户输入处理的API端点，作为AI-Agent系统的主要入口
/// Provides API endpoints for user input processing, serving as the main entry point for the AI-Agent system
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserInputController : ControllerBase
{
    private readonly IUserInputService _userInputService;    // 用户输入服务 - User input service
    private readonly IPrometheusService _prometheusService; // Prometheus监控服务 - Prometheus monitoring service

    /// <summary>
    /// Constructor with dependency injection
    /// 构造函数，使用依赖注入
    /// </summary>
    public UserInputController(IUserInputService userInputService, IPrometheusService prometheusService)
    {
        _userInputService = userInputService;
        _prometheusService = prometheusService;
    }

    /// <summary>
    /// Process user input through the AI-Agent system
    /// 通过AI-Agent系统处理用户输入
    /// </summary>
    /// <param name="request">User input request - 用户输入请求</param>
    /// <returns>Processed response - 处理后的响应</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Process user input",
        Description = "Processes user input text and orchestrates the AI agent execution flow.",
        OperationId = "ProcessInput",
        Tags = new[] { "UserInput" }
    )]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> ProcessInput([FromBody] UserInputRequest request)
    {
        // 增加请求计数器 - Increment request counter
        _prometheusService.IncrementRequestCounter("UserInput/ProcessInput");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 验证输入 - Validate input
        if (request == null || string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest(ApiResponse<string>.Fail("Input cannot be empty."));
        }

        try
        {
            // 处理用户输入 - Process user input
            var result = await _userInputService.ProcessUserInputAsync(request.Input);

            // 记录请求持续时间 - Record request duration
            stopwatch.Stop();
            _prometheusService.ObserveRequestDuration("UserInput/ProcessInput", stopwatch.Elapsed.TotalSeconds);

            // 返回处理结果 - Return processed result
            return Ok(ApiResponse<string>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail(ex.Message));
        }
    }
}

/// <summary>
/// User input request model
/// 用户输入请求模型
/// </summary>
public class UserInputRequest
{
    /// <summary>
    /// User input text
    /// 用户输入文本
    /// </summary>
    [Required]
    public string Input { get; set; } = string.Empty;
}

