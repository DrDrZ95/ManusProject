using AgentWebApi.Models.Identity;

namespace AgentWebApi.Services.Authorization;

/// <summary>
/// Permission service interface for managing user and role permissions
/// 权限服务接口，用于管理用户和角色权限
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Get all permissions for a user (including role-based and direct permissions)
    /// 获取用户的所有权限（包括基于角色的权限和直接权限）
    /// </summary>
    /// <param name="userId">User identifier - 用户标识符</param>
    /// <returns>List of permission codes - 权限代码列表</returns>
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// Check if a user has a specific permission
    /// 检查用户是否具有特定权限
    /// </summary>
    /// <param name="userId">User identifier - 用户标识符</param>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>True if user has permission - 如果用户有权限则返回true</returns>
    Task<bool> UserHasPermissionAsync(string userId, string permissionCode);

    /// <summary>
    /// Get all permissions for a role
    /// 获取角色的所有权限
    /// </summary>
    /// <param name="roleId">Role identifier - 角色标识符</param>
    /// <returns>List of permission codes - 权限代码列表</returns>
    Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId);

    /// <summary>
    /// Grant permission to a user directly
    /// 直接向用户授予权限
    /// </summary>
    /// <param name="userId">User identifier - 用户标识符</param>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>Success result - 成功结果</returns>
    Task<bool> GrantPermissionToUserAsync(string userId, string permissionCode);

    /// <summary>
    /// Revoke permission from a user directly
    /// 直接从用户撤销权限
    /// </summary>
    /// <param name="userId">User identifier - 用户标识符</param>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>Success result - 成功结果</returns>
    Task<bool> RevokePermissionFromUserAsync(string userId, string permissionCode);

    /// <summary>
    /// Grant permission to a role
    /// 向角色授予权限
    /// </summary>
    /// <param name="roleId">Role identifier - 角色标识符</param>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>Success result - 成功结果</returns>
    Task<bool> GrantPermissionToRoleAsync(string roleId, string permissionCode);

    /// <summary>
    /// Revoke permission from a role
    /// 从角色撤销权限
    /// </summary>
    /// <param name="roleId">Role identifier - 角色标识符</param>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>Success result - 成功结果</returns>
    Task<bool> RevokePermissionFromRoleAsync(string roleId, string permissionCode);

    /// <summary>
    /// Get all available permissions
    /// 获取所有可用权限
    /// </summary>
    /// <returns>List of permissions - 权限列表</returns>
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();

    /// <summary>
    /// Get permissions by category
    /// 按类别获取权限
    /// </summary>
    /// <param name="category">Permission category - 权限类别</param>
    /// <returns>List of permissions - 权限列表</returns>
    Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category);

    /// <summary>
    /// Create a new permission
    /// 创建新权限
    /// </summary>
    /// <param name="permission">Permission to create - 要创建的权限</param>
    /// <returns>Created permission - 创建的权限</returns>
    Task<Permission?> CreatePermissionAsync(Permission permission);

    /// <summary>
    /// Update an existing permission
    /// 更新现有权限
    /// </summary>
    /// <param name="permission">Permission to update - 要更新的权限</param>
    /// <returns>Updated permission - 更新的权限</returns>
    Task<Permission?> UpdatePermissionAsync(Permission permission);

    /// <summary>
    /// Delete a permission (only if not system permission)
    /// 删除权限（仅当不是系统权限时）
    /// </summary>
    /// <param name="permissionId">Permission identifier - 权限标识符</param>
    /// <returns>Success result - 成功结果</returns>
    Task<bool> DeletePermissionAsync(int permissionId);
}

