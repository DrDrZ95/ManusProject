using Microsoft.AspNetCore.Http;

namespace Agent.Core.Services.FileUpload;

/// <summary>
/// File upload service interface with OWASP security measures
/// 文件上传服务接口，包含OWASP安全措施
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Validate file upload with OWASP security measures
    /// 使用OWASP安全措施验证文件上传
    /// </summary>
    /// <param name="file">File to validate - 要验证的文件</param>
    /// <returns>Validation result - 验证结果</returns>
    Task<FileUploadValidationResult> ValidateFileAsync(IFormFile file);

    /// <summary>
    /// Get allowed file extensions
    /// 获取允许的文件扩展名
    /// </summary>
    /// <returns>List of allowed extensions - 允许的扩展名列表</returns>
    IEnumerable<string> GetAllowedExtensions();

    /// <summary>
    /// Get allowed MIME types
    /// 获取允许的MIME类型
    /// </summary>
    /// <returns>List of allowed MIME types - 允许的MIME类型列表</returns>
    IEnumerable<string> GetAllowedMimeTypes();

    /// <summary>
    /// Get maximum file size in bytes
    /// 获取最大文件大小（字节）
    /// </summary>
    /// <returns>Maximum file size - 最大文件大小</returns>
    long GetMaxFileSize();

    /// <summary>
    /// Sanitize file name to prevent path traversal attacks
    /// 清理文件名以防止路径遍历攻击
    /// </summary>
    /// <param name="fileName">Original file name - 原始文件名</param>
    /// <returns>Sanitized file name - 清理后的文件名</returns>
    string SanitizeFileName(string fileName);

    /// <summary>
    /// Scan file for malware (placeholder for antivirus integration)
    /// 扫描文件是否有恶意软件（防病毒集成的占位符）
    /// </summary>
    /// <param name="file">File to scan - 要扫描的文件</param>
    /// <returns>Scan result - 扫描结果</returns>
    Task<bool> ScanFileForMalwareAsync(IFormFile file);

    /// <summary>
    /// Generate secure file path for storage
    /// 生成安全的文件存储路径
    /// </summary>
    /// <param name="originalFileName">Original file name - 原始文件名</param>
    /// <param name="category">File category - 文件类别</param>
    /// <returns>Secure file path - 安全的文件路径</returns>
    string GenerateSecureFilePath(string originalFileName, string category);
}

/// <summary>
/// File upload validation result
/// 文件上传验证结果
/// </summary>
public class FileUploadValidationResult
{
    /// <summary>
    /// Whether the file is valid
    /// 文件是否有效
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation error messages
    /// 验证错误消息
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// File category based on type
    /// 基于类型的文件类别
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Sanitized file name
    /// 清理后的文件名
    /// </summary>
    public string SanitizedFileName { get; set; } = string.Empty;

    /// <summary>
    /// Detected MIME type
    /// 检测到的MIME类型
    /// </summary>
    public string DetectedMimeType { get; set; } = string.Empty;
}

/// <summary>
/// File upload categories
/// 文件上传类别
/// </summary>
public static class FileUploadCategories
{
    public const string Document = "documents";
    public const string Image = "images";
    public const string Video = "videos";
    public const string Audio = "audios";
    public const string Archive = "archives";
    public const string Text = "texts";
    public const string Data = "data";
    public const string Other = "others";
}

