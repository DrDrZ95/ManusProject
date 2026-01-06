namespace Agent.Application.Services;

/// <summary>
/// Service interface for file operations.
/// </summary>
public interface IFileOperationService
{
    /// <summary>
    /// Reads the content of a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The content of the file as a string.</returns>
    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes content to a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="append">Whether to append to the file or overwrite it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteFileAsync(string path, string content, bool append, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
}
