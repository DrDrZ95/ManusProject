using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Data.Entities;

/// <summary>
/// Finetune record entity for tracking model fine-tuning operations
/// 微调记录实体，用于跟踪模型微调操作
/// 
/// 记录每次微调的详细信息，包括参数、状态、结果等
/// Records detailed information for each fine-tuning operation including parameters, status, results
/// </summary>
[Table("finetune_records")]
public class FinetuneRecordEntity
{
    /// <summary>
    /// Unique identifier for the finetune record
    /// 微调记录的唯一标识符
    /// </summary>
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name/title of the fine-tuning job
    /// 微调任务的名称/标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("job_name")]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Base model name being fine-tuned
    /// 被微调的基础模型名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("base_model")]
    public string BaseModel { get; set; } = string.Empty;

    /// <summary>
    /// Dataset path or identifier used for fine-tuning
    /// 用于微调的数据集路径或标识符
    /// </summary>
    [Required]
    [MaxLength(500)]
    [Column("dataset_path")]
    public string DatasetPath { get; set; } = string.Empty;

    /// <summary>
    /// Fine-tuning configuration parameters in JSON format
    /// JSON格式的微调配置参数
    /// </summary>
    [Column("config_json", TypeName = "jsonb")]
    public string ConfigJson { get; set; } = "{}";

    /// <summary>
    /// Current status of the fine-tuning job
    /// 微调任务的当前状态
    /// </summary>
    [Required]
    [Column("status")]
    public FinetuneStatus Status { get; set; } = FinetuneStatus.NotStarted;

    /// <summary>
    /// Progress percentage (0-100)
    /// 进度百分比 (0-100)
    /// </summary>
    [Column("progress")]
    public int Progress { get; set; } = 0;

    /// <summary>
    /// Current epoch number
    /// 当前训练轮次
    /// </summary>
    [Column("current_epoch")]
    public int CurrentEpoch { get; set; } = 0;

    /// <summary>
    /// Total number of epochs
    /// 总训练轮次
    /// </summary>
    [Column("total_epochs")]
    public int TotalEpochs { get; set; } = 0;

    /// <summary>
    /// Current training loss
    /// 当前训练损失
    /// </summary>
    [Column("current_loss")]
    public double? CurrentLoss { get; set; }

    /// <summary>
    /// Best validation loss achieved
    /// 达到的最佳验证损失
    /// </summary>
    [Column("best_loss")]
    public double? BestLoss { get; set; }

    /// <summary>
    /// Learning rate used
    /// 使用的学习率
    /// </summary>
    [Column("learning_rate")]
    public double? LearningRate { get; set; }

    /// <summary>
    /// Batch size used for training
    /// 用于训练的批次大小
    /// </summary>
    [Column("batch_size")]
    public int? BatchSize { get; set; }

    /// <summary>
    /// Path to the output model directory
    /// 输出模型目录的路径
    /// </summary>
    [MaxLength(500)]
    [Column("output_path")]
    public string? OutputPath { get; set; }

    /// <summary>
    /// Training logs and output messages
    /// 训练日志和输出消息
    /// </summary>
    [Column("logs", TypeName = "text")]
    public string? Logs { get; set; }

    /// <summary>
    /// Error message if the job failed
    /// 如果任务失败的错误消息
    /// </summary>
    [Column("error_message", TypeName = "text")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Training metrics in JSON format
    /// JSON格式的训练指标
    /// </summary>
    [Column("metrics_json", TypeName = "jsonb")]
    public string? MetricsJson { get; set; }

    /// <summary>
    /// Python process ID for tracking
    /// 用于跟踪的Python进程ID
    /// </summary>
    [Column("process_id")]
    public int? ProcessId { get; set; }

    /// <summary>
    /// GPU device IDs used for training
    /// 用于训练的GPU设备ID
    /// </summary>
    [MaxLength(100)]
    [Column("gpu_devices")]
    public string? GpuDevices { get; set; }

    /// <summary>
    /// Estimated time remaining in seconds
    /// 预计剩余时间（秒）
    /// </summary>
    [Column("estimated_time_remaining")]
    public int? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Total training time in seconds
    /// 总训练时间（秒）
    /// </summary>
    [Column("total_training_time")]
    public int? TotalTrainingTime { get; set; }

    /// <summary>
    /// Memory usage in MB
    /// 内存使用量（MB）
    /// </summary>
    [Column("memory_usage_mb")]
    public int? MemoryUsageMb { get; set; }

    /// <summary>
    /// GPU memory usage in MB
    /// GPU内存使用量（MB）
    /// </summary>
    [Column("gpu_memory_usage_mb")]
    public int? GpuMemoryUsageMb { get; set; }

    /// <summary>
    /// User who initiated the fine-tuning
    /// 发起微调的用户
    /// </summary>
    [MaxLength(100)]
    [Column("created_by")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// 创建时间戳
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Job start timestamp
    /// 任务开始时间戳
    /// </summary>
    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Job completion timestamp
    /// 任务完成时间戳
    /// </summary>
    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// 最后更新时间戳
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata in JSON format
    /// JSON格式的附加元数据
    /// </summary>
    [Column("metadata_json", TypeName = "jsonb")]
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Tags for categorization and filtering
    /// 用于分类和过滤的标签
    /// </summary>
    [MaxLength(500)]
    [Column("tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Priority level of the job
    /// 任务的优先级
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; } = 5; // 1-10, 10 being highest priority

    /// <summary>
    /// Whether the job can be cancelled
    /// 任务是否可以被取消
    /// </summary>
    [Column("is_cancellable")]
    public bool IsCancellable { get; set; } = true;

    /// <summary>
    /// Whether to save checkpoints during training
    /// 是否在训练期间保存检查点
    /// </summary>
    [Column("save_checkpoints")]
    public bool SaveCheckpoints { get; set; } = true;

    /// <summary>
    /// Checkpoint save interval in epochs
    /// 检查点保存间隔（轮次）
    /// </summary>
    [Column("checkpoint_interval")]
    public int CheckpointInterval { get; set; } = 1;
}

/// <summary>
/// Finetune job status enumeration
/// 微调任务状态枚举
/// </summary>
public enum FinetuneStatus
{
    /// <summary>
    /// Job not started yet - 任务尚未开始
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Job is queued and waiting - 任务已排队等待
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Job is currently running - 任务正在运行
    /// </summary>
    Running = 2,

    /// <summary>
    /// Job is paused - 任务已暂停
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Job completed successfully - 任务成功完成
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Job failed with error - 任务失败并出错
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Job was cancelled by user - 任务被用户取消
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Job timed out - 任务超时
    /// </summary>
    Timeout = 7
}

/// <summary>
/// Extension methods for FinetuneStatus
/// FinetuneStatus的扩展方法
/// </summary>
public static class FinetuneStatusExtensions
{
    /// <summary>
    /// Check if the status indicates the job is active
    /// 检查状态是否表示任务处于活动状态
    /// </summary>
    public static bool IsActive(this FinetuneStatus status)
    {
        return status == FinetuneStatus.Queued || 
               status == FinetuneStatus.Running || 
               status == FinetuneStatus.Paused;
    }

    /// <summary>
    /// Check if the status indicates the job is finished
    /// 检查状态是否表示任务已完成
    /// </summary>
    public static bool IsFinished(this FinetuneStatus status)
    {
        return status == FinetuneStatus.Completed || 
               status == FinetuneStatus.Failed || 
               status == FinetuneStatus.Cancelled || 
               status == FinetuneStatus.Timeout;
    }

    /// <summary>
    /// Get display name for the status
    /// 获取状态的显示名称
    /// </summary>
    public static string GetDisplayName(this FinetuneStatus status)
    {
        return status switch
        {
            FinetuneStatus.NotStarted => "Not Started",
            FinetuneStatus.Queued => "Queued",
            FinetuneStatus.Running => "Running",
            FinetuneStatus.Paused => "Paused",
            FinetuneStatus.Completed => "Completed",
            FinetuneStatus.Failed => "Failed",
            FinetuneStatus.Cancelled => "Cancelled",
            FinetuneStatus.Timeout => "Timeout",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get Chinese display name for the status
    /// 获取状态的中文显示名称
    /// </summary>
    public static string GetChineseDisplayName(this FinetuneStatus status)
    {
        return status switch
        {
            FinetuneStatus.NotStarted => "未开始",
            FinetuneStatus.Queued => "排队中",
            FinetuneStatus.Running => "运行中",
            FinetuneStatus.Paused => "已暂停",
            FinetuneStatus.Completed => "已完成",
            FinetuneStatus.Failed => "失败",
            FinetuneStatus.Cancelled => "已取消",
            FinetuneStatus.Timeout => "超时",
            _ => "未知"
        };
    }
}

