namespace Agent.Core.Models.Identity;

/// <summary>
/// Application user model extending IdentityUser
/// 应用程序用户模型，扩展IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// First name of the user
    /// 用户的名字
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// 用户的姓氏
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Full name of the user
    /// 用户的全名
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Date when the user was created
    /// 用户创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user was last updated
    /// 用户最后更新日期
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the user is active
    /// 用户是否活跃
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User's profile picture URL
    /// 用户头像URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's department or organization
    /// 用户的部门或组织
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// User's job title
    /// 用户的职位
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Navigation property for user roles
    /// 用户角色的导航属性
    /// </summary>
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

    /// <summary>
    /// Navigation property for user permissions
    /// 用户权限的导航属性
    /// </summary>
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

