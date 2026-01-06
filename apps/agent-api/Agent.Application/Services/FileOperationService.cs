namespace Agent.Application.Services;

/// <summary>
/// Implementation of the file operation service.
/// </summary>
public class FileOperationService : IFileOperationService
{
    private readonly ILogger<FileOperationService> _logger;
    private readonly IConfiguration _configuration;

    public FileOperationService(ILogger<FileOperationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Reads the content of a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The content of the file as a string.</returns>
    public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reading file: {Path}", path);
        
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}", path);
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <summary>
    /// Writes content to a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="append">Whether to append to the file or overwrite it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task WriteFileAsync(string path, string content, bool append, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Writing to file: {Path}, Append: {Append}", path, append);
        
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (append)
        {
            await File.AppendAllTextAsync(path, content, cancellationToken);
        }
        else
        {
            await File.WriteAllTextAsync(path, content, cancellationToken);
        }
    }

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file: {Path}", path);
        
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}", path);
        }

        File.Delete(path);
        return Task.CompletedTask;
    }
}
