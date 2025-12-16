namespace Agent.Core.Data.Entities;

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