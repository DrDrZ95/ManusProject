namespace Agent.Core.Authorization;

/// <summary>
/// Custom authorization policy provider for permission-based authorization
/// 基于权限的自定义授权策略提供程序
/// </summary>
public class PermissionAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly ILogger<PermissionAuthorizationPolicyProvider> _logger;

    public PermissionAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        ILogger<PermissionAuthorizationPolicyProvider> logger)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _logger = logger;
    }

    /// <summary>
    /// Get authorization policy by name
    /// 按名称获取授权策略
    /// </summary>
    /// <param name="policyName">Policy name - 策略名称</param>
    /// <returns>Authorization policy - 授权策略</returns>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission-based policy
        // 检查这是否是基于权限的策略
        if (policyName.StartsWith(PermissionPolicyConstants.PERMISSION_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName.Substring(PermissionPolicyConstants.PERMISSION_POLICY_PREFIX.Length);

            _logger.LogDebug("Creating permission policy for: {PermissionCode}", permissionCode);

            var policy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("Bearer") // Require JWT Bearer authentication
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider for non-permission policies
        // 对于非权限策略，回退到默认策略提供程序
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    /// <summary>
    /// Get default authorization policy
    /// 获取默认授权策略
    /// </summary>
    /// <returns>Default authorization policy - 默认授权策略</returns>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    /// <summary>
    /// Get fallback authorization policy
    /// 获取回退授权策略
    /// </summary>
    /// <returns>Fallback authorization policy - 回退授权策略</returns>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}

/// <summary>
/// Constants for permission-based policies
/// 基于权限的策略常量
/// </summary>
public static class PermissionPolicyConstants
{
    /// <summary>
    /// Prefix for permission-based policy names
    /// 基于权限的策略名称前缀
    /// </summary>
    public const string PERMISSION_POLICY_PREFIX = "Permission.";

    /// <summary>
    /// Create a permission policy name from a permission code
    /// 从权限代码创建权限策略名称
    /// </summary>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    /// <returns>Policy name - 策略名称</returns>
    public static string CreatePermissionPolicyName(string permissionCode)
    {
        return $"{PERMISSION_POLICY_PREFIX}{permissionCode}";
    }
}

/// <summary>
/// Permission requirement for authorization
/// 授权的权限要求
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Required permission code
    /// 所需的权限代码
    /// </summary>
    public string PermissionCode { get; }

    /// <summary>
    /// Initialize permission requirement
    /// 初始化权限要求
    /// </summary>
    /// <param name="permissionCode">Permission code - 权限代码</param>
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode ?? throw new ArgumentNullException(nameof(permissionCode));
    }
}

