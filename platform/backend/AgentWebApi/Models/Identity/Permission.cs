using System.ComponentModel.DataAnnotations;

namespace AgentWebApi.Models.Identity;

/// <summary>
/// Permission model for role-based access control
/// 基于角色的访问控制权限模型
/// </summary>
public class Permission
{
    /// <summary>
    /// Unique identifier for the permission
    /// 权限的唯一标识符
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique code for the permission (e.g., "user.create", "file.upload")
    /// 权限的唯一代码（例如："user.create", "file.upload"）
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Category of the permission (e.g., "User Management", "File Operations", "System")
    /// 权限类别（例如："用户管理", "文件操作", "系统"）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the permission
    /// 权限的可读描述
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the permission is system-defined and cannot be deleted
    /// 权限是否为系统定义且不能删除
    /// </summary>
    public bool IsSystemPermission { get; set; } = false;

    /// <summary>
    /// Date when the permission was created
    /// 权限创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the permission was last updated
    /// 权限最后更新日期
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the permission is active
    /// 权限是否活跃
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for role permissions
    /// 角色权限的导航属性
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Navigation property for user permissions
    /// 用户权限的导航属性
    /// </summary>
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

/// <summary>
/// Junction table for Role-Permission many-to-many relationship
/// 角色-权限多对多关系的连接表
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Role identifier
    /// 角色标识符
    /// </summary>
    [Required]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Permission identifier
    /// 权限标识符
    /// </summary>
    [Required]
    public int PermissionId { get; set; }

    /// <summary>
    /// Date when the permission was granted to the role
    /// 权限授予角色的日期
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the role
    /// 角色的导航属性
    /// </summary>
    public virtual ApplicationRole Role { get; set; } = null!;

    /// <summary>
    /// Navigation property to the permission
    /// 权限的导航属性
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}

/// <summary>
/// Junction table for User-Permission many-to-many relationship (direct user permissions)
/// 用户-权限多对多关系的连接表（直接用户权限）
/// </summary>
public class UserPermission
{
    /// <summary>
    /// User identifier
    /// 用户标识符
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Permission identifier
    /// 权限标识符
    /// </summary>
    [Required]
    public int PermissionId { get; set; }

    /// <summary>
    /// Date when the permission was granted to the user
    /// 权限授予用户的日期
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the user
    /// 用户的导航属性
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the permission
    /// 权限的导航属性
    /// </summary>
    public virtual Permission Permission { get; set; } = null!;
}

