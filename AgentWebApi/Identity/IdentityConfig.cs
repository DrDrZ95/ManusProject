using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AgentWebApi.Identity;

/// <summary>
/// IdentityServer4 configuration for AI-Agent system
/// AI-Agent系统的IdentityServer4配置
/// 
/// 提供身份验证和授权配置，包括RSA密钥生成和基于角色的分发
/// Provides authentication and authorization configuration, including RSA key generation and role-based distribution
/// </summary>
public static class IdentityConfig
{
    /// <summary>
    /// Get identity resources
    /// 获取身份资源
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "roles",
                DisplayName = "User Roles",
                Description = "User roles for AI-Agent system - AI-Agent系统的用户角色",
                UserClaims = new List<string> { "role" }
            }
        };

    /// <summary>
    /// Get API scopes for AI-Agent system
    /// 获取AI-Agent系统的API范围
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            // AI-Agent核心API范围 - AI-Agent core API scopes
            new ApiScope("ai-agent.api", "AI-Agent API")
            {
                Description = "Full access to AI-Agent API - 完全访问AI-Agent API",
                UserClaims = new List<string> { "role", "permission" }
            },
            
            // 语义内核API范围 - Semantic Kernel API scope
            new ApiScope("semantic-kernel.api", "Semantic Kernel API")
            {
                Description = "Access to Semantic Kernel services - 访问语义内核服务",
                UserClaims = new List<string> { "role" }
            },
            
            // RAG系统API范围 - RAG system API scope
            new ApiScope("rag.api", "RAG API")
            {
                Description = "Access to RAG services - 访问RAG服务",
                UserClaims = new List<string> { "role" }
            },
            
            // 微调API范围 - Fine-tuning API scope
            new ApiScope("finetune.api", "Fine-tuning API")
            {
                Description = "Access to fine-tuning services - 访问微调服务",
                UserClaims = new List<string> { "role" }
            },
            
            // 实时通信范围 - Real-time communication scope
            new ApiScope("signalr.api", "SignalR API")
            {
                Description = "Access to real-time communication - 访问实时通信",
                UserClaims = new List<string> { "role" }
            }
        };

    /// <summary>
    /// Get API resources for AI-Agent system
    /// 获取AI-Agent系统的API资源
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("ai-agent-api", "AI-Agent API")
            {
                Description = "AI-Agent system API resource - AI-Agent系统API资源",
                Scopes = new List<string> 
                { 
                    "ai-agent.api", 
                    "semantic-kernel.api", 
                    "rag.api", 
                    "finetune.api", 
                    "signalr.api" 
                },
                UserClaims = new List<string> 
                { 
                    ClaimTypes.Name, 
                    ClaimTypes.Email, 
                    ClaimTypes.Role,
                    "permission",
                    "department"
                }
            }
        };

    /// <summary>
    /// Get clients configuration for AI-Agent system
    /// 获取AI-Agent系统的客户端配置
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // AI-Agent Web客户端 - AI-Agent Web Client
            new Client
            {
                ClientId = "ai-agent-web",
                ClientName = "AI-Agent Web Application",
                Description = "AI-Agent web frontend application - AI-Agent网页前端应用",
                
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                
                RedirectUris = 
                {
                    "https://localhost:5173/callback",
                    "https://localhost:3000/callback",
                    "http://localhost:5173/callback",
                    "http://localhost:3000/callback"
                },
                
                PostLogoutRedirectUris = 
                {
                    "https://localhost:5173/",
                    "https://localhost:3000/",
                    "http://localhost:5173/",
                    "http://localhost:3000/"
                },
                
                AllowedCorsOrigins = 
                {
                    "https://localhost:5173",
                    "https://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:3000"
                },
                
                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "email",
                    "roles",
                    "ai-agent.api",
                    "semantic-kernel.api",
                    "rag.api",
                    "finetune.api",
                    "signalr.api"
                },
                
                AllowOfflineAccess = true,
                AccessTokenLifetime = 3600, // 1小时 - 1 hour
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400 // 24小时 - 24 hours
            },
            
            // AI-Agent API客户端（用于服务间通信） - AI-Agent API Client (for service-to-service communication)
            new Client
            {
                ClientId = "ai-agent-api-client",
                ClientName = "AI-Agent API Client",
                Description = "Client for AI-Agent API service communication - AI-Agent API服务通信客户端",
                
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("ai-agent-api-secret".Sha256()) },
                
                AllowedScopes =
                {
                    "ai-agent.api",
                    "semantic-kernel.api",
                    "rag.api",
                    "finetune.api",
                    "signalr.api"
                },
                
                AccessTokenLifetime = 3600,
                Claims = new List<ClientClaim>
                {
                    new ClientClaim(ClaimTypes.Role, "ApiService"),
                    new ClientClaim("permission", "api.full_access")
                }
            },
            
            // SignalR客户端 - SignalR Client
            new Client
            {
                ClientId = "ai-agent-signalr",
                ClientName = "AI-Agent SignalR Client",
                Description = "Client for AI-Agent SignalR real-time communication - AI-Agent SignalR实时通信客户端",
                
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret = false,
                
                RedirectUris = { "https://localhost:5001/signalr-callback" },
                AllowedCorsOrigins = { "https://localhost:5001", "http://localhost:5001" },
                
                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "signalr.api"
                },
                
                AccessTokenLifetime = 7200 // 2小时 - 2 hours
            }
        };

    /// <summary>
    /// Get test users for development
    /// 获取开发用的测试用户
    /// </summary>
    public static List<TestUser> TestUsers =>
        new List<TestUser>
        {
            // 系统管理员 - System Administrator
            new TestUser
            {
                SubjectId = "admin-001",
                Username = "admin",
                Password = "admin123",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "System Administrator"),
                    new Claim(ClaimTypes.Email, "admin@ai-agent.com"),
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim("permission", "system.full_access"),
                    new Claim("department", "IT")
                }
            },
            
            // AI工程师 - AI Engineer
            new TestUser
            {
                SubjectId = "engineer-001",
                Username = "ai-engineer",
                Password = "engineer123",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "AI Engineer"),
                    new Claim(ClaimTypes.Email, "engineer@ai-agent.com"),
                    new Claim(ClaimTypes.Role, "Engineer"),
                    new Claim("permission", "ai.model_access"),
                    new Claim("permission", "ai.finetune_access"),
                    new Claim("department", "AI Research")
                }
            },
            
            // 数据科学家 - Data Scientist
            new TestUser
            {
                SubjectId = "scientist-001",
                Username = "data-scientist",
                Password = "scientist123",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Data Scientist"),
                    new Claim(ClaimTypes.Email, "scientist@ai-agent.com"),
                    new Claim(ClaimTypes.Role, "DataScientist"),
                    new Claim("permission", "data.analysis_access"),
                    new Claim("permission", "rag.query_access"),
                    new Claim("department", "Data Science")
                }
            },
            
            // 普通用户 - Regular User
            new TestUser
            {
                SubjectId = "user-001",
                Username = "user",
                Password = "user123",
                Claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Regular User"),
                    new Claim(ClaimTypes.Email, "user@ai-agent.com"),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim("permission", "basic.chat_access"),
                    new Claim("department", "General")
                }
            }
        };

    /// <summary>
    /// Generate RSA signing credentials for IdentityServer4
    /// 为IdentityServer4生成RSA签名凭据
    /// </summary>
    /// <returns>RSA signing credentials - RSA签名凭据</returns>
    public static SigningCredentials GenerateRsaSigningCredentials()
    {
        // 生成RSA密钥对 - Generate RSA key pair
        using var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa.ExportParameters(true))
        {
            KeyId = Guid.NewGuid().ToString()
        };

        return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Create persistent RSA key for production use
    /// 创建用于生产环境的持久RSA密钥
    /// </summary>
    /// <param name="keyPath">Key file path - 密钥文件路径</param>
    /// <returns>RSA signing credentials - RSA签名凭据</returns>
    public static SigningCredentials CreateOrLoadRsaKey(string keyPath)
    {
        try
        {
            // 尝试加载现有密钥 - Try to load existing key
            if (File.Exists(keyPath))
            {
                var keyData = File.ReadAllText(keyPath);
                var rsa = RSA.Create();
                rsa.ImportFromPem(keyData);
                
                var key = new RsaSecurityKey(rsa)
                {
                    KeyId = "ai-agent-rsa-key"
                };
                
                return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            }
            else
            {
                // 生成新密钥并保存 - Generate new key and save
                using var rsa = RSA.Create(2048);
                var privateKey = rsa.ExportRSAPrivateKeyPem();
                
                // 确保目录存在 - Ensure directory exists
                var directory = Path.GetDirectoryName(keyPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 保存私钥 - Save private key
                File.WriteAllText(keyPath, privateKey);
                
                // 设置文件权限（仅所有者可读） - Set file permissions (owner read only)
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                
                var key = new RsaSecurityKey(rsa.ExportParameters(true))
                {
                    KeyId = "ai-agent-rsa-key"
                };
                
                return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create or load RSA key: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Role-based authorization policies for AI-Agent system
/// AI-Agent系统的基于角色的授权策略
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Administrator policy - requires Administrator role
    /// 管理员策略 - 需要管理员角色
    /// </summary>
    public const string Administrator = "Administrator";
    
    /// <summary>
    /// AI Engineer policy - requires Engineer role or higher
    /// AI工程师策略 - 需要工程师角色或更高权限
    /// </summary>
    public const string AIEngineer = "AIEngineer";
    
    /// <summary>
    /// Data Scientist policy - requires DataScientist role or higher
    /// 数据科学家策略 - 需要数据科学家角色或更高权限
    /// </summary>
    public const string DataScientist = "DataScientist";
    
    /// <summary>
    /// Fine-tuning access policy
    /// 微调访问策略
    /// </summary>
    public const string FinetuneAccess = "FinetuneAccess";
    
    /// <summary>
    /// RAG query access policy
    /// RAG查询访问策略
    /// </summary>
    public const string RagAccess = "RagAccess";
    
    /// <summary>
    /// SignalR real-time communication access policy
    /// SignalR实时通信访问策略
    /// </summary>
    public const string SignalRAccess = "SignalRAccess";

    /// <summary>
    /// Configure authorization policies
    /// 配置授权策略
    /// </summary>
    /// <param name="options">Authorization options - 授权选项</param>
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // 管理员策略 - Administrator policy
        options.AddPolicy(Administrator, policy =>
            policy.RequireRole("Administrator"));

        // AI工程师策略 - AI Engineer policy
        options.AddPolicy(AIEngineer, policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Administrator") ||
                context.User.IsInRole("Engineer")));

        // 数据科学家策略 - Data Scientist policy
        options.AddPolicy(DataScientist, policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Administrator") ||
                context.User.IsInRole("Engineer") ||
                context.User.IsInRole("DataScientist")));

        // 微调访问策略 - Fine-tuning access policy
        options.AddPolicy(FinetuneAccess, policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Administrator") ||
                context.User.IsInRole("Engineer") ||
                context.User.HasClaim("permission", "ai.finetune_access")));

        // RAG访问策略 - RAG access policy
        options.AddPolicy(RagAccess, policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Administrator") ||
                context.User.IsInRole("Engineer") ||
                context.User.IsInRole("DataScientist") ||
                context.User.HasClaim("permission", "rag.query_access")));

        // SignalR访问策略 - SignalR access policy
        options.AddPolicy(SignalRAccess, policy =>
            policy.RequireAuthenticatedUser());
    }
}

