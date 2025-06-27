using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentWebApi.Data;

/// <summary>
/// AI-Agent PostgreSQL database context
/// AI-Agent PostgreSQL 数据库上下文
/// 
/// 提供独立的数据库访问层，支持可选的PostgreSQL集成
/// Provides independent database access layer with optional PostgreSQL integration
/// </summary>
public class AgentDbContext : DbContext
{
    public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Workflow plans table - 工作流计划表
    /// </summary>
    public DbSet<WorkflowPlanEntity> WorkflowPlans { get; set; }

    /// <summary>
    /// Workflow steps table - 工作流步骤表
    /// </summary>
    public DbSet<WorkflowStepEntity> WorkflowSteps { get; set; }

    /// <summary>
    /// Vector documents table - 向量文档表
    /// </summary>
    public DbSet<VectorDocumentEntity> VectorDocuments { get; set; }

    /// <summary>
    /// Chat sessions table - 聊天会话表
    /// </summary>
    public DbSet<ChatSessionEntity> ChatSessions { get; set; }

    /// <summary>
    /// Chat messages table - 聊天消息表
    /// </summary>
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    /// <summary>
    /// Audit logs table - 审计日志表
    /// </summary>
    public DbSet<AuditLogEntity> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置工作流计划实体 - Configure workflow plan entity
        modelBuilder.Entity<WorkflowPlanEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.ExecutorKeys).HasColumnType("jsonb");
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UpdatedAt);
        });

        // 配置工作流步骤实体 - Configure workflow step entity
        modelBuilder.Entity<WorkflowStepEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Result).HasMaxLength(2000);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.Status);
            
            // 外键关系 - Foreign key relationship
            entity.HasOne<WorkflowPlanEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置向量文档实体 - Configure vector document entity
        modelBuilder.Entity<VectorDocumentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CollectionName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DocumentId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.Embedding).HasColumnType("vector(1536)"); // 假设使用OpenAI embeddings
            entity.HasIndex(e => e.CollectionName);
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // 配置聊天会话实体 - Configure chat session entity
        modelBuilder.Entity<ChatSessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // 配置聊天消息实体 - Configure chat message entity
        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
            
            // 外键关系 - Foreign key relationship
            entity.HasOne<ChatSessionEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置审计日志实体 - Configure audit log entity
        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Changes).HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}

#region Entity Models - 实体模型

/// <summary>
/// Workflow plan entity for database storage
/// 用于数据库存储的工作流计划实体
/// </summary>
[Table("workflow_plans")]
public class WorkflowPlanEntity
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column(TypeName = "jsonb")]
    public string? ExecutorKeys { get; set; }

    public int? CurrentStepIndex { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Workflow step entity for database storage
/// 用于数据库存储的工作流步骤实体
/// </summary>
[Table("workflow_steps")]
public class WorkflowStepEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string PlanId { get; set; } = string.Empty;

    public int StepIndex { get; set; }

    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Type { get; set; }

    public int Status { get; set; } // PlanStepStatus enum as int

    [MaxLength(2000)]
    public string? Result { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Vector document entity for database storage
/// 用于数据库存储的向量文档实体
/// </summary>
[Table("vector_documents")]
public class VectorDocumentEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string CollectionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DocumentId { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column(TypeName = "vector(1536)")]
    public float[]? Embedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Chat session entity for database storage
/// 用于数据库存储的聊天会话实体
/// </summary>
[Table("chat_sessions")]
public class ChatSessionEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Chat message entity for database storage
/// 用于数据库存储的聊天消息实体
/// </summary>
[Table("chat_messages")]
public class ChatMessageEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // user, assistant, system

    [Required]
    public string Content { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Audit log entity for database storage
/// 用于数据库存储的审计日志实体
/// </summary>
[Table("audit_logs")]
public class AuditLogEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UserId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Changes { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

#endregion

