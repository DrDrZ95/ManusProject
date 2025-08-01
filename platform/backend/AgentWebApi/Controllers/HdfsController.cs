using AgentWebApi.Services.Hdfs;
using AgentWebApi.Services.FileUpload;
using Microsoft.AspNetCore.Mvc;
using AgentWebApi.Services.Telemetry;
using System.Text;

namespace AgentWebApi.Controllers;

/// <summary>
/// HDFS 文件处理控制器 - HDFS File Processing Controller
/// 提供文件上传、分类和存储到 HDFS 的示例，包含OWASP安全措施 - Provides examples for file upload, classification, and storage to HDFS with OWASP security measures
/// </summary>
[ApiController]
[Route("api/hdfs")]
public class HdfsController : ControllerBase
{
    private readonly IHdfsService _hdfsService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<HdfsController> _logger;
    private readonly IAgentTelemetryProvider _telemetryProvider;

    public HdfsController(
        IHdfsService hdfsService, 
        IFileUploadService fileUploadService,
        ILogger<HdfsController> logger, 
        IAgentTelemetryProvider telemetryProvider)
    {
        _hdfsService = hdfsService;
        _fileUploadService = fileUploadService;
        _logger = logger;
        _telemetryProvider = telemetryProvider;
    }

    /// <summary>
    /// 上传并分类存储文件到 HDFS - Upload and classify files to HDFS
    /// 包含OWASP安全验证 - Includes OWASP security validation
    /// </summary>
    /// <param name="file">要上传的文件 - The file to upload</param>
    /// <returns>上传结果 - Upload result</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        using (var span = _telemetryProvider.StartSpan("HdfsController.UploadFile"))
        {
            span.SetAttribute("hdfs.file_name", file?.FileName);
            span.SetAttribute("hdfs.file_size", file?.Length);
            span.SetAttribute("hdfs.content_type", file?.ContentType);

            if (file == null || file.Length == 0)
            {
                span.SetStatus(ActivityStatusCode.Error, "No file uploaded.");
                return BadRequest(new { Error = "No file uploaded - 未上传文件" });
            }

            try
            {
                // OWASP Security Validation - OWASP安全验证
                _logger.LogInformation("Starting OWASP security validation for file: {FileName}", file.FileName);
                var validationResult = await _fileUploadService.ValidateFileAsync(file);
                
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("File validation failed for {FileName}: {Errors}", 
                        file.FileName, string.Join(", ", validationResult.ErrorMessages));
                    span.SetStatus(ActivityStatusCode.Error, "File validation failed");
                    return BadRequest(new 
                    { 
                        Error = "File validation failed - 文件验证失败", 
                        Details = validationResult.ErrorMessages 
                    });
                }

                span.SetAttribute("hdfs.file_category", validationResult.Category);
                span.SetAttribute("hdfs.sanitized_file_name", validationResult.SanitizedFileName);

                // Generate secure file path using OWASP-compliant service
                // 使用符合OWASP的服务生成安全文件路径
                var secureFilePath = _fileUploadService.GenerateSecureFilePath(
                    validationResult.SanitizedFileName, 
                    validationResult.Category);

                string remoteDirectory = Path.GetDirectoryName(secureFilePath).Replace('\\', '/') + "/";
                string remotePath = secureFilePath.Replace('\\', '/');

                span.SetAttribute("hdfs.remote_directory", remoteDirectory);
                span.SetAttribute("hdfs.remote_path", remotePath);

                // Ensure the remote directory exists
                // 确保远程目录存在
                await _hdfsService.CreateDirectoryAsync(remoteDirectory);

                // Upload file stream with validated content
                // 使用验证过的内容上传文件流
                using (var stream = file.OpenReadStream())
                {
                    bool success = await _hdfsService.UploadFileAsync(remotePath, stream, validationResult.DetectedMimeType);
                    if (success)
                    {
                        _logger.LogInformation("File {FileName} (sanitized: {SanitizedFileName}) uploaded to HDFS at {RemotePath}", 
                            file.FileName, validationResult.SanitizedFileName, remotePath);
                        span.SetAttribute("hdfs.upload_success", true);
                        
                        return Ok(new 
                        { 
                            Message = "File uploaded successfully - 文件上传成功", 
                            Path = remotePath,
                            Category = validationResult.Category,
                            OriginalFileName = file.FileName,
                            SanitizedFileName = validationResult.SanitizedFileName,
                            FileSize = file.Length,
                            MimeType = validationResult.DetectedMimeType
                        });
                    }
                    else
                    {
                        _logger.LogError("Failed to upload file {FileName} to HDFS.", validationResult.SanitizedFileName);
                        span.SetStatus(ActivityStatusCode.Error, "Failed to upload file to HDFS.");
                        return StatusCode(500, new { Error = "Failed to upload file to HDFS - HDFS文件上传失败" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to HDFS: {FileName}", file?.FileName);
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, new { Error = $"Internal server error - 内部服务器错误: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// 接收 JSON 数据并存储到 HDFS - Receive JSON data and store to HDFS
    /// </summary>
    /// <param name="jsonData">JSON 数据 - JSON data</param>
    /// <param name="fileName">文件名 (可选) - File name (optional)</param>
    /// <returns>存储结果 - Storage result</returns>
    [HttpPost("json")]
    [Consumes("application/json")]
    public async Task<IActionResult> ReceiveJson([FromBody] object jsonData, [FromQuery] string? fileName = null)
    {
        using (var span = _telemetryProvider.StartSpan("HdfsController.ReceiveJson"))
        {
            span.SetAttribute("hdfs.file_name", fileName);
            if (jsonData == null)
            {
                span.SetStatus(ActivityStatusCode.Error, "No JSON data provided.");
                return BadRequest("No JSON data provided.");
            }

            try
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                using (var stream = new MemoryStream(byteArray))
                {
                    string remoteDirectory = "/user/ai-agent/json_data/";
                    string finalFileName = fileName ?? $"json_data_{DateTime.UtcNow.Ticks}.json";
                    string remotePath = Path.Combine(remoteDirectory, finalFileName);

                    span.SetAttribute("hdfs.remote_directory", remoteDirectory);
                    span.SetAttribute("hdfs.remote_path", remotePath);

                    await _hdfsService.CreateDirectoryAsync(remoteDirectory);
                    bool success = await _hdfsService.UploadFileAsync(remotePath, stream, "application/json");

                    if (success)
                    {
                        _logger.LogInformation("JSON data stored to HDFS at {RemotePath}", remotePath);
                        span.SetAttribute("hdfs.upload_success", true);
                        return Ok(new { Message = "JSON data stored successfully.", Path = remotePath });
                    }
                    else
                    {
                        _logger.LogError("Failed to store JSON data to HDFS.");
                        span.SetStatus(ActivityStatusCode.Error, "Failed to store JSON data to HDFS.");
                        return StatusCode(500, "Failed to store JSON data to HDFS.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving or storing JSON data to HDFS.");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 接收二进制流数据并存储到 HDFS - Receive binary stream data and store to HDFS
    /// </summary>
    /// <param name="fileName">文件名 - File name</n>
    /// <param name="contentType">内容类型 - Content type</param>
    /// <returns>存储结果 - Storage result</returns>
    [HttpPost("stream/{fileName}")]
    public async Task<IActionResult> ReceiveStream(string fileName, [FromHeader(Name = "Content-Type")] string contentType)
    {
        using (var span = _telemetryProvider.StartSpan("HdfsController.ReceiveStream"))
        {
            span.SetAttribute("hdfs.file_name", fileName);
            span.SetAttribute("hdfs.content_type", contentType);

            if (string.IsNullOrEmpty(fileName))
            {
                span.SetStatus(ActivityStatusCode.Error, "File name is required.");
                return BadRequest("File name is required.");
            }

            if (Request.Body == null || !Request.Body.CanRead)
            {
                span.SetStatus(ActivityStatusCode.Error, "No stream data provided.");
                return BadRequest("No stream data provided.");
            }

            try
            {
                string remoteDirectory = "/user/ai-agent/stream_data/";
                string remotePath = Path.Combine(remoteDirectory, fileName);

                span.SetAttribute("hdfs.remote_directory", remoteDirectory);
                span.SetAttribute("hdfs.remote_path", remotePath);

                await _hdfsService.CreateDirectoryAsync(remoteDirectory);
                bool success = await _hdfsService.UploadFileAsync(remotePath, Request.Body, contentType);

                if (success)
                {
                    _logger.LogInformation("Stream data {FileName} stored to HDFS at {RemotePath}", fileName, remotePath);
                    span.SetAttribute("hdfs.upload_success", true);
                    return Ok(new { Message = "Stream data stored successfully.", Path = remotePath });
                }
                else
                {
                    _logger.LogError("Failed to store stream data {FileName} to HDFS.", fileName);
                    span.SetStatus(ActivityStatusCode.Error, "Failed to store stream data to HDFS.");
                    return StatusCode(500, "Failed to store stream data to HDFS.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving or storing stream data to HDFS.");
                span.RecordException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


