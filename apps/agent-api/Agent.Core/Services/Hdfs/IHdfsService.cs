namespace Agent.Core.Services.Hdfs
{
    public interface IHdfsService
    {
        Task<bool> UploadFileAsync(string remotePath, Stream fileStream, string contentType);
        Task<Stream> DownloadFileAsync(string remotePath);
        Task<string> ReadTextFileAsync(string remotePath);
        Task<bool> CreateDirectoryAsync(string remotePath);
        Task<bool> FileExistsAsync(string remotePath);
        Task<bool> DeleteFileAsync(string remotePath);
    }
}


