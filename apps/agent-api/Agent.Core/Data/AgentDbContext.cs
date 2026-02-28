namespace Agent.Core.Data;

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

    /// <summary>
    /// Token usage records table - Token 使用记录表
    /// </summary>
    public DbSet<TokenUsageRecord> TokenUsageRecords { get; set; }

    /// <summary>
    /// Tool metadata table - 工具元数据表
    /// </summary>
    public DbSet<ToolMetadataEntity> ToolMetadata { get; set; }

    /// <summary>
    /// Finetune records table - 微调记录表
    /// </summary>
    public DbSet<FinetuneRecordEntity> FinetuneRecords { get; set; }

    public DbSet<AgentTraceEntity> AgentTraces { get; set; }

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
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
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

        // 配置工具元数据实体 - Configure tool metadata entity
        modelBuilder.Entity<ToolMetadataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ParameterSchema).HasColumnType("jsonb");
            entity.Property(e => e.Permissions).HasColumnType("jsonb");
            entity.Property(e => e.CostInfo).HasColumnType("jsonb");
            entity.Property(e => e.ReliabilityMetrics).HasColumnType("jsonb");
            entity.Property(e => e.Dependencies).HasColumnType("jsonb");
            entity.Property(e => e.Configuration).HasColumnType("jsonb");
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsEnabled);
        });

        // 配置微调记录实体 - Configure finetune record entity
        modelBuilder.Entity<FinetuneRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.JobName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BaseModel).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DatasetPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ConfigJson).HasColumnType("jsonb");
            entity.Property(e => e.MetricsJson).HasColumnType("jsonb");
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.Logs).HasColumnType("text");
            entity.Property(e => e.ErrorMessage).HasColumnType("text");
            entity.Property(e => e.OutputPath).HasMaxLength(500);
            entity.Property(e => e.GpuDevices).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);

            // 索引配置 - Index configuration
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.BaseModel);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.CompletedAt);
            entity.HasIndex(e => e.Priority);
        });

        modelBuilder.Entity<AgentTraceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TraceId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SessionId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Data).HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CostUsd).HasColumnType("numeric(10,4)");
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
        });

        // 配置 Token 使用记录实体 - Configure Token usage record entity
        modelBuilder.Entity<TokenUsageRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModelId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.ModelId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

