using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Identity;

namespace Agent.Core.Data;

/// <summary>
/// Application database context for Identity and PostgreSQL
/// 应用程序数据库上下文，用于Identity和PostgreSQL
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Permissions DbSet
    /// 权限数据集
    /// </summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>
    /// Role permissions DbSet
    /// 角色权限数据集
    /// </summary>
    public DbSet<RolePermission> RolePermissions { get; set; }

    /// <summary>
    /// User permissions DbSet
    /// 用户权限数据集
    /// </summary>
    public DbSet<UserPermission> UserPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure table names with custom schema
        // 配置表名和自定义架构
        builder.HasDefaultSchema("identity");

        // Configure ApplicationUser
        // 配置ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            
            // Index for performance
            // 性能索引
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.FirstName, e.LastName });
        });

        // Configure ApplicationRole
        // 配置ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
            
            // Index for performance
            // 性能索引
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Permission
        // 配置Permission
        builder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            
            // Index for performance
            // 性能索引
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure RolePermission (Many-to-Many)
        // 配置RolePermission（多对多）
        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Index for performance
            // 性能索引
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.PermissionId);
        });

        // Configure UserPermission (Many-to-Many)
        // 配置UserPermission（多对多）
        builder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => new { e.UserId, e.PermissionId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Index for performance
            // 性能索引
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PermissionId);
        });

        // Configure Identity tables
        // 配置Identity表
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Seed default data (optional)
        // 种子默认数据（可选）
        SeedDefaultData(builder);
    }

    /// <summary>
    /// Seed default data for roles and permissions
    /// 为角色和权限播种默认数据
    /// </summary>
    private void SeedDefaultData(ModelBuilder builder)
    {
        // Seed default roles
        // 播种默认角色
        var adminRoleId = Guid.NewGuid().ToString();
        var userRoleId = Guid.NewGuid().ToString();
        var moderatorRoleId = Guid.NewGuid().ToString();

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = adminRoleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                Description = "Full system access with all permissions",
                Category = "System",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ApplicationRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user with basic permissions",
                Category = "System",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ApplicationRole
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR",
                Description = "Moderator with elevated permissions",
                Category = "System",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed default permissions
        // 播种默认权限
        builder.Entity<Permission>().HasData(
            // User Management Permissions
            // 用户管理权限
            new Permission { Id = 1, Code = "user.create", Category = "User Management", Description = "Create new users", IsSystemPermission = true },
            new Permission { Id = 2, Code = "user.read", Category = "User Management", Description = "View user information", IsSystemPermission = true },
            new Permission { Id = 3, Code = "user.update", Category = "User Management", Description = "Update user information", IsSystemPermission = true },
            new Permission { Id = 4, Code = "user.delete", Category = "User Management", Description = "Delete users", IsSystemPermission = true },
            
            // Role Management Permissions
            // 角色管理权限
            new Permission { Id = 5, Code = "role.create", Category = "Role Management", Description = "Create new roles", IsSystemPermission = true },
            new Permission { Id = 6, Code = "role.read", Category = "Role Management", Description = "View role information", IsSystemPermission = true },
            new Permission { Id = 7, Code = "role.update", Category = "Role Management", Description = "Update role information", IsSystemPermission = true },
            new Permission { Id = 8, Code = "role.delete", Category = "Role Management", Description = "Delete roles", IsSystemPermission = true },
            
            // File Operations Permissions
            // 文件操作权限
            new Permission { Id = 9, Code = "file.upload", Category = "File Operations", Description = "Upload files to the system", IsSystemPermission = true },
            new Permission { Id = 10, Code = "file.download", Category = "File Operations", Description = "Download files from the system", IsSystemPermission = true },
            new Permission { Id = 11, Code = "file.delete", Category = "File Operations", Description = "Delete files from the system", IsSystemPermission = true },
            
            // System Administration Permissions
            // 系统管理权限
            new Permission { Id = 12, Code = "system.admin", Category = "System Administration", Description = "Full system administration access", IsSystemPermission = true },
            new Permission { Id = 13, Code = "system.monitor", Category = "System Administration", Description = "Monitor system performance and health", IsSystemPermission = true },
            new Permission { Id = 14, Code = "system.config", Category = "System Administration", Description = "Configure system settings", IsSystemPermission = true },
            
            // API Access Permissions
            // API访问权限
            new Permission { Id = 15, Code = "api.read", Category = "API Access", Description = "Read access to API endpoints", IsSystemPermission = true },
            new Permission { Id = 16, Code = "api.write", Category = "API Access", Description = "Write access to API endpoints", IsSystemPermission = true },
            new Permission { Id = 17, Code = "api.admin", Category = "API Access", Description = "Administrative access to API endpoints", IsSystemPermission = true }
        );

        // Seed role permissions
        // 播种角色权限
        builder.Entity<RolePermission>().HasData(
            // Administrator gets all permissions
            // 管理员获得所有权限
            new RolePermission { RoleId = adminRoleId, PermissionId = 1 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 2 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 3 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 4 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 5 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 6 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 7 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 8 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 9 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 10 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 11 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 12 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 13 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 14 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 15 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 16 },
            new RolePermission { RoleId = adminRoleId, PermissionId = 17 },
            
            // User gets basic permissions
            // 用户获得基本权限
            new RolePermission { RoleId = userRoleId, PermissionId = 2 }, // user.read
            new RolePermission { RoleId = userRoleId, PermissionId = 9 }, // file.upload
            new RolePermission { RoleId = userRoleId, PermissionId = 10 }, // file.download
            new RolePermission { RoleId = userRoleId, PermissionId = 15 }, // api.read
            
            // Moderator gets elevated permissions
            // 版主获得提升权限
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 2 }, // user.read
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 3 }, // user.update
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 6 }, // role.read
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 9 }, // file.upload
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 10 }, // file.download
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 11 }, // file.delete
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 13 }, // system.monitor
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 15 }, // api.read
            new RolePermission { RoleId = moderatorRoleId, PermissionId = 16 } // api.write
        );
    }
}

