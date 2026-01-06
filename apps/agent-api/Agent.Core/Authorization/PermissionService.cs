namespace Agent.Core.Authorization;

/// <summary>
/// Permission service implementation for managing user and role permissions
/// 权限服务实现，用于管理用户和角色权限
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(ApplicationDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            // Get permissions from user roles
            // 从用户角色获取权限
            var rolePermissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
                .Join(_context.Permissions, rp => rp, p => p.Id, (rp, p) => p.Code)
                .Where(code => _context.Permissions.Any(p => p.Code == code && p.IsActive))
                .ToListAsync();

            // Get direct user permissions
            // 获取直接用户权限
            var directPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Join(_context.Permissions, up => up.PermissionId, p => p.Id, (up, p) => p.Code)
                .Where(code => _context.Permissions.Any(p => p.Code == code && p.IsActive))
                .ToListAsync();

            // Combine and return unique permissions
            // 合并并返回唯一权限
            var allPermissions = rolePermissions.Concat(directPermissions).Distinct();
            
            _logger.LogInformation("Retrieved {Count} permissions for user {UserId}", allPermissions.Count(), userId);
            return allPermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserHasPermissionAsync(string userId, string permissionCode)
    {
        try
        {
            // Check role-based permissions
            // 检查基于角色的权限
            var hasRolePermission = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
                .Join(_context.Permissions, rp => rp, p => p.Id, (rp, p) => p)
                .AnyAsync(p => p.Code == permissionCode && p.IsActive);

            if (hasRolePermission)
            {
                return true;
            }

            // Check direct user permissions
            // 检查直接用户权限
            var hasDirectPermission = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Join(_context.Permissions, up => up.PermissionId, p => p.Id, (up, p) => p)
                .AnyAsync(p => p.Code == permissionCode && p.IsActive);

            return hasDirectPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionCode} for user {UserId}", permissionCode, userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId)
    {
        try
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
                .Where(code => _context.Permissions.Any(p => p.Code == code && p.IsActive))
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} permissions for role {RoleId}", permissions.Count, roleId);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for role {RoleId}", roleId);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> GrantPermissionToUserAsync(string userId, string permissionCode)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

            if (permission == null)
            {
                _logger.LogWarning("Permission {PermissionCode} not found", permissionCode);
                return false;
            }

            var existingUserPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.Id);

            if (existingUserPermission != null)
            {
                _logger.LogInformation("User {UserId} already has permission {PermissionCode}", userId, permissionCode);
                return true;
            }

            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permission.Id,
                GrantedAt = DateTime.UtcNow
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Granted permission {PermissionCode} to user {UserId}", permissionCode, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission {PermissionCode} to user {UserId}", permissionCode, userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RevokePermissionFromUserAsync(string userId, string permissionCode)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permissionCode);

            if (permission == null)
            {
                _logger.LogWarning("Permission {PermissionCode} not found", permissionCode);
                return false;
            }

            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.Id);

            if (userPermission == null)
            {
                _logger.LogInformation("User {UserId} does not have direct permission {PermissionCode}", userId, permissionCode);
                return true;
            }

            _context.UserPermissions.Remove(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked permission {PermissionCode} from user {UserId}", permissionCode, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {PermissionCode} from user {UserId}", permissionCode, userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> GrantPermissionToRoleAsync(string roleId, string permissionCode)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

            if (permission == null)
            {
                _logger.LogWarning("Permission {PermissionCode} not found", permissionCode);
                return false;
            }

            var existingRolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

            if (existingRolePermission != null)
            {
                _logger.LogInformation("Role {RoleId} already has permission {PermissionCode}", roleId, permissionCode);
                return true;
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id,
                GrantedAt = DateTime.UtcNow
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Granted permission {PermissionCode} to role {RoleId}", permissionCode, roleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission {PermissionCode} to role {RoleId}", permissionCode, roleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RevokePermissionFromRoleAsync(string roleId, string permissionCode)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permissionCode);

            if (permission == null)
            {
                _logger.LogWarning("Permission {PermissionCode} not found", permissionCode);
                return false;
            }

            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permission.Id);

            if (rolePermission == null)
            {
                _logger.LogInformation("Role {RoleId} does not have permission {PermissionCode}", roleId, permissionCode);
                return true;
            }

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked permission {PermissionCode} from role {RoleId}", permissionCode, roleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {PermissionCode} from role {RoleId}", permissionCode, roleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active permissions", permissions.Count);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all permissions");
            return Enumerable.Empty<Permission>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => p.Category == category && p.IsActive)
                .OrderBy(p => p.Code)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} permissions for category {Category}", permissions.Count, category);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for category {Category}", category);
            return Enumerable.Empty<Permission>();
        }
    }

    /// <inheritdoc />
    public async Task<Permission?> CreatePermissionAsync(Permission permission)
    {
        try
        {
            // Check if permission code already exists
            // 检查权限代码是否已存在
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == permission.Code);

            if (existingPermission != null)
            {
                _logger.LogWarning("Permission with code {PermissionCode} already exists", permission.Code);
                return null;
            }

            permission.CreatedAt = DateTime.UtcNow;
            permission.UpdatedAt = DateTime.UtcNow;

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new permission {PermissionCode} in category {Category}", permission.Code, permission.Category);
            return permission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission {PermissionCode}", permission.Code);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Permission?> UpdatePermissionAsync(Permission permission)
    {
        try
        {
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            if (existingPermission == null)
            {
                _logger.LogWarning("Permission with ID {PermissionId} not found", permission.Id);
                return null;
            }

            // Don't allow updating system permissions' core properties
            // 不允许更新系统权限的核心属性
            if (existingPermission.IsSystemPermission)
            {
                _logger.LogWarning("Cannot update system permission {PermissionCode}", existingPermission.Code);
                return null;
            }

            existingPermission.Description = permission.Description;
            existingPermission.Category = permission.Category;
            existingPermission.IsActive = permission.IsActive;
            existingPermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated permission {PermissionCode}", existingPermission.Code);
            return existingPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", permission.Id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeletePermissionAsync(int permissionId)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
            {
                _logger.LogWarning("Permission with ID {PermissionId} not found", permissionId);
                return false;
            }

            if (permission.IsSystemPermission)
            {
                _logger.LogWarning("Cannot delete system permission {PermissionCode}", permission.Code);
                return false;
            }

            // Remove all role and user permissions first
            // 首先删除所有角色和用户权限
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync();

            var userPermissions = await _context.UserPermissions
                .Where(up => up.PermissionId == permissionId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);
            _context.UserPermissions.RemoveRange(userPermissions);
            _context.Permissions.Remove(permission);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted permission {PermissionCode} and all associated assignments", permission.Code);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", permissionId);
            return false;
        }
    }
}

