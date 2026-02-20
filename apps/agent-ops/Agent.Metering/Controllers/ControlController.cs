namespace Agent.Metering.Controllers;

/// <summary>
/// 控制面 API，用于为 SWE-Agent 提供健康检查与场景控制能力
/// </summary>
[ApiController]
[Route("control")]
public class ControlController : ControllerBase
{
    private readonly IMeteringHealthSnapshotProvider _healthProvider;
    private readonly IScenarioManager _scenarioManager;

    /// <summary>
    /// 通过依赖注入获取流水线健康快照提供器和场景管理器
    /// </summary>
    public ControlController(
        IMeteringHealthSnapshotProvider healthProvider,
        IScenarioManager scenarioManager)
    {
        _healthProvider = healthProvider;
        _scenarioManager = scenarioManager;
    }

    /// <summary>
    /// 返回当前 Agent.Metering 的健康状态及活动场景信息
    /// </summary>
    [HttpGet("health")]
    public ActionResult<MeteringHealthSnapshot> GetHealth()
    {
        var snapshot = _healthProvider.GetSnapshot();
        return Ok(snapshot);
    }

    /// <summary>
    /// 返回自监控指标快照（当前与健康快照结构一致，后续可扩展为更细粒度 metrics）
    /// </summary>
    [HttpGet("metrics")]
    public ActionResult<MeteringHealthSnapshot> GetMetrics()
    {
        var snapshot = _healthProvider.GetSnapshot();
        return Ok(snapshot);
    }

    /// <summary>
    /// 启动指定名称的采集场景（可用于分场景压测或定向观测）
    /// </summary>
    [HttpPost("scenarios/{name}/start")]
    public async Task<IActionResult> StartScenario(string name, CancellationToken cancellationToken)
    {
        await _scenarioManager.StartScenarioAsync(name, cancellationToken);
        return Accepted($"/control/scenarios/{name}");
    }

    /// <summary>
    /// 停止指定名称的采集场景
    /// </summary>
    [HttpPost("scenarios/{name}/stop")]
    public async Task<IActionResult> StopScenario(string name, CancellationToken cancellationToken)
    {
        await _scenarioManager.StopScenarioAsync(name, cancellationToken);
        return Accepted($"/control/scenarios/{name}");
    }
}

