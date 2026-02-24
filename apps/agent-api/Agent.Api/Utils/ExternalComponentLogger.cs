namespace Agent.Api.Utils;

/// <summary>
/// 专门用于记录和显示外部组件（如 Redis, Database, HDFS 等）异常的工具类。
/// 按照用户要求，将这些异常以黄色加粗的控制台提示输出，并提供修复建议。
/// </summary>
public static class ExternalComponentLogger
{
    private static readonly object _consoleLock = new object();

    public static void LogConnectionError(string componentName, Exception ex, string? fixSuggestion = null)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            
            // 模拟加粗（控制台本身对加粗支持有限，通常通过高亮色模拟）
            Console.WriteLine("********************************************************************************");
            Console.WriteLine($"[EXTERNAL COMPONENT ERROR] Component: {componentName}");
            Console.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            
            if (!string.IsNullOrEmpty(fixSuggestion))
            {
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine($"FIX SUGGESTION: {fixSuggestion}");
            }
            
            Console.WriteLine("Note: The application will continue to run, but related features may be degraded.");
            Console.WriteLine("********************************************************************************");
            Console.WriteLine();

            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// 根据异常类型自动识别组件并记录错误
    /// </summary>
    public static void HandleException(Exception ex)
    {
        var (componentName, suggestion) = IdentifyComponent(ex);
        if (!string.IsNullOrEmpty(componentName))
        {
            LogConnectionError(componentName, ex, suggestion);
        }
    }

    private static (string componentName, string suggestion) IdentifyComponent(Exception ex)
    {
        var typeName = ex.GetType().FullName ?? "";
        var message = ex.Message.ToLower();

        if (typeName.Contains("StackExchange.Redis") || message.Contains("redis"))
        {
            return ("Redis Cache", "请检查 Redis 服务是否启动，网络连接是否正常，以及连接字符串配置。");
        }
        
        if (typeName.Contains("Npgsql") || message.Contains("postgresql") || message.Contains("postgre"))
        {
            return ("PostgreSQL Database", "请检查 PostgreSQL 服务是否在线，数据库是否存在，以及连接字符串中的用户名/密码。");
        }

        if (message.Contains("hdfs") || message.Contains("hadoop"))
        {
            return ("HDFS Storage", "请检查 HDFS 集群状态及 NameNode 连通性。");
        }

        if (typeName.Contains("ChromaDB") || message.Contains("chroma"))
        {
            return ("Chroma Vector Database", "请检查 ChromaDB Docker 容器或远程服务是否正常运行。");
        }

        if (typeName.Contains("Dapr") || message.Contains("dapr"))
        {
            return ("Dapr Runtime", "请确保已通过 'dapr run' 启动 Sidecar。");
        }

        return (string.Empty, string.Empty);
    }
}
