using Python.Runtime;
using System.Diagnostics;
using System.Text.Json;
using Agent.Core.Data.Entities;

namespace Agent.Core.Services.Finetune;

/// <summary>
/// Python.NET finetune service interface
/// Python.NET微调服务接口
/// 
/// 提供与Python微调脚本交互的服务接口
/// Provides service interface for interacting with Python fine-tuning scripts
/// </summary>
public interface IPythonFinetuneService
{
    /// <summary>
    /// Start a new fine-tuning job
    /// 启动新的微调任务
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Job ID - 任务ID</returns>
    Task<string> StartFinetuningAsync(FinetuneRequest request);

    /// <summary>
    /// Get fine-tuning job status
    /// 获取微调任务状态
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job status - 任务状态</returns>
    Task<FinetuneRecordEntity?> GetJobStatusAsync(string jobId);

    /// <summary>
    /// Cancel a running fine-tuning job
    /// 取消正在运行的微调任务
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> CancelJobAsync(string jobId);

    /// <summary>
    /// Get all fine-tuning jobs
    /// 获取所有微调任务
    /// </summary>
    /// <param name="status">Filter by status - 按状态过滤</param>
    /// <param name="limit">Maximum number of results - 最大结果数</param>
    /// <returns>List of jobs - 任务列表</returns>
    Task<List<FinetuneRecordEntity>> GetJobsAsync(FinetuneStatus? status = null, int limit = 100);

    /// <summary>
    /// Get job logs
    /// 获取任务日志
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <returns>Job logs - 任务日志</returns>
    Task<string?> GetJobLogsAsync(string jobId);

    /// <summary>
    /// Update job progress
    /// 更新任务进度
    /// </summary>
    /// <param name="jobId">Job ID - 任务ID</param>
    /// <param name="progress">Progress update - 进度更新</param>
    /// <returns>Success status - 成功状态</returns>
    Task<bool> UpdateJobProgressAsync(string jobId, FinetuneProgress progress);

    /// <summary>
    /// Validate Python environment and dependencies
    /// 验证Python环境和依赖项
    /// </summary>
    /// <returns>Validation result - 验证结果</returns>
    Task<PythonEnvironmentInfo> ValidatePythonEnvironmentAsync();

    /// <summary>
    /// Get available models for fine-tuning
    /// 获取可用于微调的模型
    /// </summary>
    /// <returns>List of available models - 可用模型列表</returns>
    Task<List<string>> GetAvailableModelsAsync();

    /// <summary>
    /// Estimate training time and resources
    /// 估算训练时间和资源
    /// </summary>
    /// <param name="request">Fine-tuning request - 微调请求</param>
    /// <returns>Resource estimation - 资源估算</returns>
    Task<ResourceEstimation> EstimateResourcesAsync(FinetuneRequest request);
}

/// <summary>
/// Fine-tuning request model
/// 微调请求模型
/// </summary>
public class FinetuneRequest
{
    /// <summary>
    /// Job name - 任务名称
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Base model to fine-tune - 要微调的基础模型
    /// </summary>
    public string BaseModel { get; set; } = "Qwen/Qwen3-4B-Instruct";

    /// <summary>
    /// Dataset path - 数据集路径
    /// </summary>
    public string DatasetPath { get; set; } = string.Empty;

    /// <summary>
    /// Output directory - 输出目录
    /// </summary>
    public string OutputDir { get; set; } = string.Empty;

    /// <summary>
    /// Number of training epochs - 训练轮次
    /// </summary>
    public int Epochs { get; set; } = 3;

    /// <summary>
    /// Learning rate - 学习率
    /// </summary>
    public double LearningRate { get; set; } = 2e-5;

    /// <summary>
    /// Batch size - 批次大小
    /// </summary>
    public int BatchSize { get; set; } = 4;

    /// <summary>
    /// Maximum sequence length - 最大序列长度
    /// </summary>
    public int MaxLength { get; set; } = 512;

    /// <summary>
    /// GPU device IDs - GPU设备ID
    /// </summary>
    public string? GpuDevices { get; set; }

    /// <summary>
    /// Save checkpoints - 保存检查点
    /// </summary>
    public bool SaveCheckpoints { get; set; } = true;

    /// <summary>
    /// Checkpoint save steps - 检查点保存步数
    /// </summary>
    public int SaveSteps { get; set; } = 500;

    /// <summary>
    /// Evaluation steps - 评估步数
    /// </summary>
    public int EvalSteps { get; set; } = 500;

    /// <summary>
    /// Logging steps - 日志记录步数
    /// </summary>
    public int LoggingSteps { get; set; } = 100;

    /// <summary>
    /// Warmup steps - 预热步数
    /// </summary>
    public int WarmupSteps { get; set; } = 100;

    /// <summary>
    /// Weight decay - 权重衰减
    /// </summary>
    public double WeightDecay { get; set; } = 0.01;

    /// <summary>
    /// Additional configuration - 附加配置
    /// </summary>
    public Dictionary<string, object>? AdditionalConfig { get; set; }

    /// <summary>
    /// Tags for categorization - 分类标签
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Priority level (1-10) - 优先级 (1-10)
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// Created by user - 创建用户
    /// </summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Fine-tuning progress update model
/// 微调进度更新模型
/// </summary>
public class FinetuneProgress
{
    /// <summary>
    /// Current epoch - 当前轮次
    /// </summary>
    public int CurrentEpoch { get; set; }

    /// <summary>
    /// Total epochs - 总轮次
    /// </summary>
    public int TotalEpochs { get; set; }

    /// <summary>
    /// Progress percentage (0-100) - 进度百分比 (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Current training loss - 当前训练损失
    /// </summary>
    public double? CurrentLoss { get; set; }

    /// <summary>
    /// Best validation loss - 最佳验证损失
    /// </summary>
    public double? BestLoss { get; set; }

    /// <summary>
    /// Estimated time remaining in seconds - 预计剩余时间（秒）
    /// </summary>
    public int? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Memory usage in MB - 内存使用量（MB）
    /// </summary>
    public int? MemoryUsageMb { get; set; }

    /// <summary>
    /// GPU memory usage in MB - GPU内存使用量（MB）
    /// </summary>
    public int? GpuMemoryUsageMb { get; set; }

    /// <summary>
    /// Training metrics - 训练指标
    /// </summary>
    public Dictionary<string, object>? Metrics { get; set; }

    /// <summary>
    /// Log messages - 日志消息
    /// </summary>
    public List<string>? LogMessages { get; set; }

    public decimal ProgressPercentage { get; set; }
}

/// <summary>
/// Python environment information
/// Python环境信息
/// </summary>
public class PythonEnvironmentInfo
{
    /// <summary>
    /// Python version - Python版本
    /// </summary>
    public string PythonVersion { get; set; } = string.Empty;

    /// <summary>
    /// Python executable path - Python可执行文件路径
    /// </summary>
    public string PythonPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether environment is valid - 环境是否有效
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Installed packages - 已安装的包
    /// </summary>
    public Dictionary<string, string> InstalledPackages { get; set; } = new();

    /// <summary>
    /// Missing required packages - 缺少的必需包
    /// </summary>
    public List<string> MissingPackages { get; set; } = new();

    /// <summary>
    /// Available GPU devices - 可用的GPU设备
    /// </summary>
    public List<GpuInfo> AvailableGpus { get; set; } = new();

    /// <summary>
    /// Error messages - 错误消息
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    public bool IsPythonInstalled { get; set; } = false;
}

/// <summary>
/// GPU information
/// GPU信息
/// </summary>
public class GpuInfo
{
    /// <summary>
    /// GPU device ID - GPU设备ID
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// GPU name - GPU名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Total memory in MB - 总内存（MB）
    /// </summary>
    public long TotalMemoryMb { get; set; }

    /// <summary>
    /// Available memory in MB - 可用内存（MB）
    /// </summary>
    public long AvailableMemoryMb { get; set; }

    /// <summary>
    /// GPU utilization percentage - GPU利用率百分比
    /// </summary>
    public int UtilizationPercent { get; set; }
}

/// <summary>
/// Resource estimation for fine-tuning
/// 微调的资源估算
/// </summary>
public class ResourceEstimation
{
    /// <summary>
    /// Estimated training time in hours - 预计训练时间（小时）
    /// </summary>
    public double EstimatedTrainingHours { get; set; }

    /// <summary>
    /// Required GPU memory in MB - 所需GPU内存（MB）
    /// </summary>
    public long RequiredGpuMemoryMb { get; set; }

    /// <summary>
    /// Required system memory in MB - 所需系统内存（MB）
    /// </summary>
    public long RequiredSystemMemoryMb { get; set; }

    /// <summary>
    /// Required disk space in MB - 所需磁盘空间（MB）
    /// </summary>
    public long RequiredDiskSpaceMb { get; set; }

    /// <summary>
    /// Recommended GPU count - 推荐GPU数量
    /// </summary>
    public int RecommendedGpuCount { get; set; }

    /// <summary>
    /// Whether resources are sufficient - 资源是否充足
    /// </summary>
    public bool ResourcesSufficient { get; set; }

    /// <summary>
    /// Resource warnings - 资源警告
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Optimization suggestions - 优化建议
    /// </summary>
    public List<string> Suggestions { get; set; } = new();

    public object? EstimatedGpuMemoryMb { get; set; }
    public object? EstimatedTrainingTimeMinutes { get; set; }
}

