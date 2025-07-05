using AgentWebApi.Services.Hdfs;
using Microsoft.AspNetCore.Mvc;

namespace AgentWebApi.Controllers;

/// <summary>
/// HDFS 文件处理控制器 - HDFS File Processing Controller
/// 提供文件上传、分类和存储到 HDFS 的示例 - Provides examples for file upload, classification, and storage to HDFS
/// </summary>
[ApiController]
[Route("api/hdfs")]
public class HdfsController : ControllerBase
{
    private readonly IHdfsService _hdfsService;
    private readonly ILogger<HdfsController> _logger;

    public HdfsController(IHdfsService hdfsService, ILogger<HdfsController> logger)
    {
        _hdfsService = hdfsService;
        _logger = logger;
    }

    /// <summary>
    /// 上传并分类存储文件到 HDFS - Upload and classify files to HDFS
    /// </summary>
    /// <param name="file">要上传的文件 - The file to upload</param>
    /// <returns>上传结果 - Upload result</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string contentType = file.ContentType;
            string remoteDirectory = "/user/ai-agent/uploads/";
            string remotePath;

            // 1. 根据文件类型分类存储 - Classify and store based on file type
            if (contentType.Contains("json") || fileExtension == ".json")
            {
                remoteDirectory += "json/";
            }
            else if (contentType.Contains("image") || fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif")
            {
                remoteDirectory += "images/";
            }
            else if (contentType.Contains("video") || fileExtension == ".mp4" || fileExtension == ".avi")
            {
                remoteDirectory += "videos/";
            }
            else if (contentType.Contains("text") || fileExtension == ".txt" || fileExtension == ".log")
            {
                remoteDirectory += "texts/";
            }
            else
            {
                remoteDirectory += "others/";
            }

            // 确保远程目录存在 - Ensure the remote directory exists
            await _hdfsService.CreateDirectoryAsync(remoteDirectory);

            remotePath = Path.Combine(remoteDirectory, file.FileName);

            // 2. 上传文件流 - Upload file stream
            using (var stream = file.OpenReadStream())
            {
                bool success = await _hdfsService.UploadFileAsync(remotePath, stream, contentType);
                if (success)
                {
                    _logger.LogInformation("File {FileName} uploaded to HDFS at {RemotePath}", file.FileName, remotePath);
                    return Ok(new { Message = "File uploaded successfully.", Path = remotePath });
                }
                else
                {
                    _logger.LogError("Failed to upload file {FileName} to HDFS.", file.FileName);
                    return StatusCode(500, "Failed to upload file to HDFS.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to HDFS.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
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
        if (jsonData == null)
        {
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

                await _hdfsService.CreateDirectoryAsync(remoteDirectory);
                bool success = await _hdfsService.UploadFileAsync(remotePath, stream, "application/json");

                if (success)
                {
                    _logger.LogInformation("JSON data stored to HDFS at {RemotePath}", remotePath);
                    return Ok(new { Message = "JSON data stored successfully.", Path = remotePath });
                }
                else
                {
                    _logger.LogError("Failed to store JSON data to HDFS.");
                    return StatusCode(500, "Failed to store JSON data to HDFS.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving or storing JSON data to HDFS.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 接收二进制流数据并存储到 HDFS - Receive binary stream data and store to HDFS
    /// </summary>
    /// <param name="fileName">文件名 - File name</param>
    /// <param name="contentType">内容类型 - Content type</param>
    /// <returns>存储结果 - Storage result</returns>
    [HttpPost("stream/{fileName}")]
    public async Task<IActionResult> ReceiveStream(string fileName, [FromHeader(Name = "Content-Type")] string contentType)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }

        if (Request.Body == null || !Request.Body.CanRead)
        {
            return BadRequest("No stream data provided.");
        }

        try
        {
            string remoteDirectory = "/user/ai-agent/stream_data/";
            string remotePath = Path.Combine(remoteDirectory, fileName);

            await _hdfsService.CreateDirectoryAsync(remoteDirectory);
            bool success = await _hdfsService.UploadFileAsync(remotePath, Request.Body, contentType);

            if (success)
            {
                _logger.LogInformation("Stream data {FileName} stored to HDFS at {RemotePath}", fileName, remotePath);
                return Ok(new { Message = "Stream data stored successfully.", Path = remotePath });
            }
            else
            {
                _logger.LogError("Failed to store stream data {FileName} to HDFS.", fileName);
                return StatusCode(500, "Failed to store stream data to HDFS.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving or storing stream data to HDFS.");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}


