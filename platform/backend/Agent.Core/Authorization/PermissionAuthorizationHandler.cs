using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agent.Core.Authorization;

namespace Agent.Core.Authorization;

/// <summary>
/// Authorization handler for permission-based authorization
/// 基于权限的授权处理程序
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionService permissionService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Handle authorization requirement
    /// 处理授权要求
    /// </summary>
    /// <param name="context">Authorization context - 授权上下文</param>
    /// <param name="requirement">Permission requirement - 权限要求</param>
    /// <returns>Task - 任务</returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        try
        {
            // Get user ID from claims
            // 从声明中获取用户ID
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims for permission check: {PermissionCode}", requirement.PermissionCode);
                context.Fail();
                return;
            }

            // Check if user has the required permission
            // 检查用户是否具有所需权限
            var hasPermission = await _permissionService.UserHasPermissionAsync(userId, requirement.PermissionCode);
            
            if (hasPermission)
            {
                _logger.LogDebug("User {UserId} has permission {PermissionCode}", userId, requirement.PermissionCode);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have permission {PermissionCode}", userId, requirement.PermissionCode);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionCode} for user", requirement.PermissionCode);
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization attribute for permission-based authorization
/// 基于权限的授权特性
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initialize permission authorization attribute
    /// 初始化权限授权特性
    /// </summary>
    /// <param name="permissionCode">Required permission code - 所需的权限代码</param>
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = PermissionPolicyConstants.CreatePermissionPolicyName(permissionCode);
    }
}

/// <summary>
/// Multiple permissions authorization attribute (requires ALL permissions)
/// 多权限授权特性（需要所有权限）
/// </summary>
public class RequireAllPermissionsAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initialize multiple permissions authorization attribute
    /// 初始化多权限授权特性
    /// </summary>
    /// <param name="permissionCodes">Required permission codes - 所需的权限代码</param>
    public RequireAllPermissionsAttribute(params string[] permissionCodes)
    {
        if (permissionCodes == null || permissionCodes.Length == 0)
        {
            throw new ArgumentException("At least one permission code is required", nameof(permissionCodes));
        }

        // Create a comma-separated policy name for multiple permissions
        // 为多个权限创建逗号分隔的策略名称
        var policyNames = permissionCodes.Select(PermissionPolicyConstants.CreatePermissionPolicyName);
        Policy = string.Join(",", policyNames);
    }
}

/// <summary>
/// Authorization handler for multiple permissions (requires ALL permissions)
/// 多权限授权处理程序（需要所有权限）
/// </summary>
public class MultiplePermissionsAuthorizationHandler : AuthorizationHandler<MultiplePermissionsRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<MultiplePermissionsAuthorizationHandler> _logger;

    public MultiplePermissionsAuthorizationHandler(
        IPermissionService permissionService,
        ILogger<MultiplePermissionsAuthorizationHandler> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Handle multiple permissions authorization requirement
    /// 处理多权限授权要求
    /// </summary>
    /// <param name="context">Authorization context - 授权上下文</param>
    /// <param name="requirement">Multiple permissions requirement - 多权限要求</param>
    /// <returns>Task - 任务</returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultiplePermissionsRequirement requirement)
    {
        try
        {
            // Get user ID from claims
            // 从声明中获取用户ID
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims for multiple permissions check");
                context.Fail();
                return;
            }

            // Check if user has ALL required permissions
            // 检查用户是否具有所有所需权限
            var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
            var hasAllPermissions = requirement.PermissionCodes.All(code => userPermissions.Contains(code));
            
            if (hasAllPermissions)
            {
                _logger.LogDebug("User {UserId} has all required permissions: {PermissionCodes}", 
                    userId, string.Join(", ", requirement.PermissionCodes));
                context.Succeed(requirement);
            }
            else
            {
                var missingPermissions = requirement.PermissionCodes.Except(userPermissions);
                _logger.LogWarning("User {UserId} is missing permissions: {MissingPermissions}", 
                    userId, string.Join(", ", missingPermissions));
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking multiple permissions for user");
            context.Fail();
        }
    }
}

/// <summary>
/// Multiple permissions requirement for authorization
/// 授权的多权限要求
/// </summary>
public class MultiplePermissionsRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Required permission codes (ALL must be present)
    /// 所需的权限代码（必须全部存在）
    /// </summary>
    public IEnumerable<string> PermissionCodes { get; }

    /// <summary>
    /// Initialize multiple permissions requirement
    /// 初始化多权限要求
    /// </summary>
    /// <param name="permissionCodes">Permission codes - 权限代码</param>
    public MultiplePermissionsRequirement(IEnumerable<string> permissionCodes)
    {
        PermissionCodes = permissionCodes ?? throw new ArgumentNullException(nameof(permissionCodes));
    }
}

