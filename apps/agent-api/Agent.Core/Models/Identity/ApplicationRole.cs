using Microsoft.AspNetCore.Identity;

namespace Agent.Core.Models.Identity;

/// <summary>
/// Application role model extending IdentityRole
/// 应用程序角色模型，扩展IdentityRole
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Description of the role
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category of the role (e.g., System, Business, Custom)
    /// 角色类别（例如：系统、业务、自定义）
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether the role is system-defined and cannot be deleted
    /// 角色是否为系统定义且不能删除
    /// </summary>
    public bool IsSystemRole { get; set; } = false;

    /// <summary>
    /// Date when the role was created
    /// 角色创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the role was last updated
    /// 角色最后更新日期
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the role is active
    /// 角色是否活跃
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for role permissions
    /// 角色权限的导航属性
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Navigation property for user roles
    /// 用户角色的导航属性
    /// </summary>
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
}

